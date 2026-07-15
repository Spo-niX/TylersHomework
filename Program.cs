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

var proxy = new WebProxy("socks5://127.0.0.1:1088");

// 2. Создаём Handler с кастомным резолвингом
var handler = new HttpClientHandler
{
    Proxy = proxy,
    UseProxy = true
};
List<string> AgentNames = new List<string>
{
    "ALPHA", "BRAVO", "CHARLIE", "DELTA", "ECHO", 
    "FOXTROT", "GOLF", "HOTEL", "INDIA", "JULIET"
};
handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
var socketsHandler = new SocketsHttpHandler
{
    Proxy = proxy,
    UseProxy = true,
    ConnectCallback = async (context, cancellationToken) =>
    {
        // Если запрос к api.telegram.org — подменяем IP
        if (context.DnsEndPoint.Host == "api.telegram.org")
        {
            var ipAddress = IPAddress.Parse("149.154.167.99");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(ipAddress, context.DnsEndPoint.Port);
            return new NetworkStream(socket, ownsSocket: true);
        }
        // Для всех остальных запросов — стандартное поведение
        var defaultSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await defaultSocket.ConnectAsync(context.DnsEndPoint.Host, context.DnsEndPoint.Port);
        return new NetworkStream(defaultSocket, ownsSocket: true);
    }
};

var httpClient = new HttpClient(socketsHandler);

var builder = Host.CreateApplicationBuilder();

string token = builder.Configuration["token"]!; 

var dbPath = "bot.db";
DatabaseConnection.Initialize(dbPath);

builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<CallbackHandlerHelp>();

var ServiceProvider = builder.Services.BuildServiceProvider();
var CallbackHandler = ServiceProvider.GetRequiredService<CallbackHandlerHelp>();
var _userRepo = ServiceProvider.GetRequiredService<UserRepository>();
var _taskRepo = ServiceProvider.GetRequiredService<UserTaskRepository>();

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
            if(state == "waitName")
            {
                if (text!.Length < 3 || text.Length > 10 || !text.All(x => char.IsLetter(x)))
                {
                    await client.SendTextMessageAsync(
                        chatId, "Позывной не прошёл валидацию. Пожалуйста, попробуйте ещё раз", 
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

                    await client.SendTextMessageAsync(
                        chatId, 
                        "Позывной успешно прошёл валидацию!", 
                        cancellationToken: cancellationToken);
                }
            }
            else if(state == "waitId")
            {
                var matchJS = await httpClient.GetAsync($"https://api.opendota.com/api/matches/{text}");
                
                if (!matchJS.IsSuccessStatusCode)
                {
                    var errorContent = await matchJS.Content.ReadAsStringAsync();
                    if (matchJS.StatusCode == HttpStatusCode.NotFound)
                    {
                        await client.SendTextMessageAsync(
                            chatId,
                            "Матч по такому ID не был найден. Проверьте ID",
                            cancellationToken: cancellationToken
                        );
                        return;
                    }
                    Console.WriteLine($"Ошибка API!!!!!! {matchJS.StatusCode} - {errorContent}");
                    await client.SendTextMessageAsync(
                            chatId,
                            "Внешнаяя ошибка API. Агентсво борется над её устранением ",
                            cancellationToken: cancellationToken
                        );
                    return;
                }
                
                var match = JsonSerializer.Deserialize<MatchData>(matchJS.ToString());

                var agentSteam = await _userRepo.stId(message.From.Id);
                var agPlr = match!.Players.FirstOrDefault(x => x.AccountId == agentSteam);
                var agPos = match.Players.IndexOf(agPlr!);
                var agent = await _userRepo.GetByTelegramIdAsync(message.From.Id);
                var rnd = new Random();

                if(agPlr == null)
                {
                    await client.SendTextMessageAsync(
                        chatId,
                        "Ваше присутствие в игре не обнаружено! Проверьте ID матча",
                        cancellationToken: cancellationToken
                    );
                    return;
                }
                UserStates.ClearState(message.From.Id);

                if(!((match.RadiantWin && agPos <= 4) || (!match.RadiantWin && agPos >= 5)))
                {
                    await client.SendTextMessageAsync(
                        chatId,
                        "Вы проиграли! Задание провалено!",
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

                UserTask task = await _taskRepo.GetByIdAsync(message.From.Id);
                if(!task.Slots!.Contains(CallbackHandler.GetItemName(agPlr.Item0)) ||
                !task.Slots!.Contains(CallbackHandler.GetItemName(agPlr.Item1)) ||
                !task.Slots!.Contains(CallbackHandler.GetItemName(agPlr.Item2)) ||
                !task.Slots!.Contains(CallbackHandler.GetItemName(agPlr.Item3)) ||
                !task.Slots!.Contains(CallbackHandler.GetItemName(agPlr.Item4)) ||
                !task.Slots!.Contains(CallbackHandler.GetItemName(agPlr.Item5)))
                {
                    UserTaskState.ClearState(message.From.Id);

                    agent.Mmr -= 25 + rnd.Next(-5, 6);
                    if(agent.Mmr < 0)
                    {
                        agent.Mmr = 0; 
                    }

                    await client.SendTextMessageAsync(
                        chatId,
                        "Обнаружено несоответствие ваших предметов, с предметами в задании! Задание провалено!",
                        replyMarkup: GetExKB(),
                        cancellationToken: cancellationToken
                    );
                    return;
                }

                await client.SendTextMessageAsync(
                        chatId,
                        "Задание выполнено успешно! Поздравляю, агент!",
                        replyMarkup: GetExKB(),
                        cancellationToken: cancellationToken
                    );
                agent.Mmr += 25 + rnd.Next(-5, 6);
                agent.TaskCompleted++;
                task.IsActive = false;
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
                text: "Неверный формат данных",
                cancellationToken: cancellationToken);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка в обработчике: {ex.Message}");
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
            new[] {InlineKeyboardButton.WithCallbackData("Вернуться в меню", "menu")}
        }
    );
    
}