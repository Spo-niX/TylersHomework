using System.Net;
using System.Text.Json;
using SQLitePCL;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TylersHomework.Core;
using TylersHomework.Core.Database.Models;
using TylersHomework.Core.Database.Repositories;

public class UpdateHandler
{
    private readonly ITelegramBotClient _client;
    private readonly Update _update;
    private readonly CancellationToken _cancellationToken;
    private readonly CallbackHandlerHelp _callbackHandler;
    private readonly HttpClient _httpClient;
    private readonly CommandExecutor _commandExecutor;
    private readonly UserRepository _userRepo;
    private readonly UserTaskRepository _taskRepo;

    public UpdateHandler(ITelegramBotClient client, Update update, CancellationToken cancellationToken,
    CallbackHandlerHelp callbackHandler, HttpClient httpClient, CommandExecutor commandExecutor,
    UserRepository userRepo, UserTaskRepository taskRepo)
    {
        _client = client;
        _update = update;
        _cancellationToken = cancellationToken;
        _callbackHandler = callbackHandler;
        _httpClient = httpClient;
        _commandExecutor = commandExecutor;
        _userRepo = userRepo;
        _taskRepo = taskRepo;
    }

    public async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Получено обновление: {update.Type}");
        try
        {
            if (update.Message is { } message)
            {
                var chatId = message.Chat.Id;
                var text = message.Text;

                var state = UserStates.GetState(message.From!.Id);
                if(state == "waitName" && text![0] != '/')
                {
                    if (text!.Length < 3 || text.Length > 10 || !text.All(x => char.IsLetter(x)))
                    {
                        await client.SendTextMessageAsync(
                            chatId, "✖︎ Позывной не прошёл валидацию. Пожалуйста, попробуйте ещё раз ✖︎", 
                            cancellationToken: cancellationToken);
                        return;
                    }
                    else
                    {
                        if (!await _userRepo.ExistsAsync(message.From.Id))
                        {
                            await createAgent(message.From.Id, text);
                        }
                        
                        UserStates.ClearState(message.From.Id);
                        UserStates.SetState(message.From.Id, "waitSteam");

                        await client.SendTextMessageAsync(
                            chatId, 
                            "✔︎ Позывной успешно прошёл валидацию! Теперь, отправьте ваш Steam ID ✔︎", 
                            cancellationToken: cancellationToken);
                    }
                }
                else if(state == "waitSteam" && text![0] != '/')
                {
                    if (!text!.All(x => char.IsDigit(x)))
                    {
                        await client.SendTextMessageAsync(
                            chatId, "✖︎ Неверный формат. Попробуйте ещё раз! ✖︎", 
                            cancellationToken: cancellationToken);
                        return;
                    }
                    else
                    {
                        var agent = await _userRepo.GetByTelegramIdAsync(message.From.Id);
                        agent.SteamId = Convert.ToInt64(text);
                        await _userRepo.SaveAsync(agent);
                        UserStates.ClearState(message.From.Id);

                        await client.SendTextMessageAsync(
                            chatId, 
                            "✔︎ Ваша регистрация успешно закончена! ✔︎",
                            replyMarkup: GetExKB(), 
                            cancellationToken: cancellationToken);
                    }
                }
                else if(state == "waitId" && text![0] != '/')
                {
                    var matchJS = await _httpClient.GetAsync($"https://api.opendota.com/api/matches/{text}");
                    var js = await matchJS.Content.ReadAsStringAsync();
                    
                    if (!matchJS.IsSuccessStatusCode)
                    {
                        var errorContent = await matchJS.Content.ReadAsStringAsync();
                        if (matchJS.StatusCode == HttpStatusCode.NotFound)
                        {
                            await client.SendTextMessageAsync(
                                chatId,
                                "✖︎ Матч по такому ID не был найден. Проверьте ID ✖︎",
                                cancellationToken: cancellationToken
                            );
                            return;
                        }
                        Console.WriteLine($"Ошибка API!!!!!! {matchJS.StatusCode} - {errorContent}");
                        await client.SendTextMessageAsync(
                                chatId,
                                "✖︎ Внешнаяя ошибка API. Агентсво борется над её устранением ✖︎",
                                cancellationToken: cancellationToken
                            );
                        return;
                    }
                    
                    using var doc = JsonDocument.Parse(js.ToString());
                    var root = doc.RootElement;
                    
                    var players = root.GetProperty("players");
                    var heroId = -1;
                    short isWin = 0;
                    bool isFound = false;

                    List<string> items = new List<string>();
                    JsonElement agJs = new JsonElement();

                    UserTask task = await _taskRepo.GetByIdAsync(message.From.Id);
                    if(task == null)
                    {
                        await client.SendTextMessageAsync(
                            chatId,
                            "✖︎ Задание не найдено! Возьмите новое ✖︎",
                            cancellationToken: cancellationToken
                        );
                        return; 
                    }
                    foreach (var player in players.EnumerateArray())
                    {
                        heroId = player.GetProperty("hero_id").GetInt32();
                        if(true)
                        {
                            isFound = true;
                            agJs = player;
                            player.TryGetProperty("win", out JsonElement winElement);

                            isWin = winElement.GetInt16();
                            
                            for (int i = 0; i <= 5; i++)
                            {
                                var itemProp = player.GetProperty($"item_{i}");
                                items.Add(_callbackHandler.GetItemName(itemProp.GetInt32()));
                            }
                        }
                    }

                    if(!isFound)
                    {
                        await client.SendTextMessageAsync(
                            chatId,
                            "✖︎ Ваше присутствие в игре не обнаружено! Проверьте ID матча ✖︎",
                            cancellationToken: cancellationToken
                        );
                        return;    
                    }         
                
                    var agent = await _userRepo.GetByTelegramIdAsync(message.From.Id);
                    var rnd = new Random();

                    UserStates.ClearState(message.From.Id);

                    if(isWin == 0)
                    {
                        await client.SendTextMessageAsync(
                            chatId,
                            "✖︎ Вы проиграли! Задание провалено! ✖︎",
                            replyMarkup: GetExKB(),
                            cancellationToken: cancellationToken
                        );

                        UserTaskState.ClearState(message.From.Id);

                        agent.Mmr -= 25 + rnd.Next(-5, 6);
                        if(agent.Mmr < 0)
                        {
                            agent.Mmr = 0; 
                        }
                        return;
                    }

                    if(!items.All(x => task.Slots!.Contains(x) || _callbackHandler.GetItemId(x) == -1))
                    {
                        UserTaskState.ClearState(message.From.Id);

                        agent.Mmr -= 25 + rnd.Next(-5, 6);
                        if(agent.Mmr < 0)
                        {
                            agent.Mmr = 0; 
                        }

                        await client.SendTextMessageAsync(
                            chatId,
                            "✖︎ Обнаружено несоответствие ваших предметов, с предметами в задании! Задание провалено! ✖︎",
                            replyMarkup: GetExKB(),
                            cancellationToken: cancellationToken
                        );
                        return;
                    }

                    await client.SendTextMessageAsync(
                            chatId,
                            "✔︎ Задание выполнено успешно! Поздравляю, агент! ✔︎",
                            replyMarkup: GetExKB(),
                            cancellationToken: cancellationToken
                        );
                    agent.Mmr += 25 + rnd.Next(-5, 6);
                    agent.TaskCompleted++;
                    task.IsActive = false;
                    await _userRepo.SaveAsync(agent);
                    await _taskRepo.SaveAsync(task);
                }
                else if(state == null && text![0] == '/')
                {
                    await _commandExecutor.ExecuteAsync(client, message, cancellationToken);
                }
            }
            else if(update.Type == Telegram.Bot.Types.Enums.UpdateType.CallbackQuery)
            {
                await _callbackHandler.HandleHelp(client, update.CallbackQuery!, cancellationToken);
            }
            else
            {
                await _client.SendTextMessageAsync(
                    chatId: update.Message!.Chat.Id,
                    text: "✖︎ Неверный формат данных ✖︎",
                    cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✖︎ Ошибка в обработчике: {ex.Message}");
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient client, Exception ex, CancellationToken ct)
    {
        Console.WriteLine("=== POLLING ERROR ===");
        var e = ex;
        int depth = 0;
        while (e != null)
        {
            Console.WriteLine($"[{depth}] {e.GetType().FullName}: {e.Message}");
            if (e is HttpRequestException hre && hre.StatusCode != null)
                Console.WriteLine($"    StatusCode: {hre.StatusCode}");
            e = e.InnerException;
            depth++;
        }
        return Task.CompletedTask;
    }
    
    
    
    
    
    async Task createAgent(long tgId, string text)
    {
        List<string> AgentNames = new List<string>
        {
            "ALPHA", "BRAVO", "CHARLIE", "DELTA", "ECHO", 
            "FOXTROT", "GOLF", "HOTEL", "INDIA", "JULIET"
        };
        await _userRepo.SaveAsync(new TylersHomework.Core.Database.Models.User
        {
            TgId = tgId,
            AgentName = AgentNames[await _userRepo.GetCountAsync() % AgentNames.Count] 
                + ' ' + $"00{await _userRepo.GetCountAsync() + 1}".PadLeft(4, '0') + $@" ""{text}""",
            Mmr = 0,
            TaskCompleted = 0,

        });
    }
    private InlineKeyboardMarkup GetExKB()
    {
        return new InlineKeyboardMarkup(
            new[]
            {
                new[] {InlineKeyboardButton.WithCallbackData("◀︎ В меню", "menu")}
            }
        );
    }
}

