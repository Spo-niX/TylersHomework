using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TylersHomework.Core;  
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using TylersHomework.Core.Database;
using TylersHomework.Core.Database.Repositories;
using System.Net;
using System.Net.Sockets;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net.Http.Headers;
using Telegram.Bot.Requests;
using System.Text.Json;
using TylersHomework.Core.Database.Models;

List<string> AgentNames = new List<string>
{
    "ALPHA", "BRAVO", "CHARLIE", "DELTA", "ECHO", 
    "FOXTROT", "GOLF", "HOTEL", "INDIA", "JULIET"
};

var builder = Host.CreateApplicationBuilder();
builder.Configuration.AddEnvironmentVariables();
var httpClient = new HttpClient();

builder.Configuration.AddUserSecrets<Program>();


if(builder.Configuration["useProxy"] == "yes")
{
    var proxy = new WebProxy("socks5://127.0.0.1:1088");

    var handler = new HttpClientHandler
    {
        Proxy = proxy,
        UseProxy = true
    };

    handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
    var socketsHandler = new SocketsHttpHandler
    {
        Proxy = proxy,
        UseProxy = true,
        ConnectCallback = async (context, cancellationToken) =>
        {
            if (context.DnsEndPoint.Host == "api.telegram.org")
            {
                var ipAddress = IPAddress.Parse("149.154.167.99");
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await socket.ConnectAsync(ipAddress, context.DnsEndPoint.Port);
                return new NetworkStream(socket, ownsSocket: true);
            }
            var defaultSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await defaultSocket.ConnectAsync(context.DnsEndPoint.Host, context.DnsEndPoint.Port);
            return new NetworkStream(defaultSocket, ownsSocket: true);
        }
    };

    httpClient = new HttpClient(socketsHandler);
}

string token = builder.Configuration["token"];
var dbPath = "bot.db";
DatabaseConnection.Initialize(dbPath);

builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<UserTaskRepository>();
builder.Services.AddSingleton<CallbackHandlerHelp>();

var ServiceProvider = builder.Services.BuildServiceProvider();
var CallbackHandler = ServiceProvider.GetRequiredService<CallbackHandlerHelp>();
var _userRepo = ServiceProvider.GetRequiredService<UserRepository>();
var _taskRepo = ServiceProvider.GetRequiredService<UserTaskRepository>();

var app = builder.Build();
var botClient = new TelegramBotClient(token, httpClient);
var commandExecutor = new CommandExecutor(ServiceProvider);  

using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>() 
};

botClient.StartReceiving(
    updateHandler: HandleUpdate,
    pollingErrorHandler: HandleErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token
);
UserTaskState.ClearAllState();
UserStates.ClearAllState();
Console.ReadLine();
cts.Cancel();
async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken cancellationToken)
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
                var matchJS = await httpClient.GetAsync($"https://api.opendota.com/api/matches/{text}");
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
                            items.Add(CallbackHandler.GetItemName(itemProp.GetInt32()));
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

                if(!items.All(x => task.Slots!.Contains(x) || CallbackHandler.GetItemId(x) == -1))
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
            else
            {
                await commandExecutor.ExecuteAsync(client, message, cancellationToken);
            }
        }
        else if(update.Type == UpdateType.CallbackQuery)
        {
            await CallbackHandler.HandleHelp(client, update.CallbackQuery!, cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
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

Task HandleErrorAsync(ITelegramBotClient client, Exception exception, CancellationToken cancellationToken)
{
    Console.WriteLine($"ошибка: {exception.Message}");
    return Task.CompletedTask;
}

async Task createAgent(long tgId, string text)
{
    await _userRepo.SaveAsync(new TylersHomework.Core.Database.Models.User
    {
        TgId = tgId,
        AgentName = AgentNames[await _userRepo.GetCountAsync() % AgentNames.Count] 
            + ' ' + $"00{await _userRepo.GetCountAsync() + 1}".PadLeft(4, '0') + $@" ""{text}""",
        Mmr = 0,
        TaskCompleted = 0,

    });
}

InlineKeyboardMarkup GetExKB()
{
    return new InlineKeyboardMarkup(
        new[]
        {
            new[] {InlineKeyboardButton.WithCallbackData("◀︎ В меню", "menu")}
        }
    );
}