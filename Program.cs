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

var proxy = new WebProxy("socks5://127.0.0.1:1088");

// 2. Создаём Handler с кастомным резолвингом
var handler = new HttpClientHandler
{
    Proxy = proxy,
    UseProxy = true
};

// 3. Принудительно подменяем DNS
handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
// Создаём кастомный SocketsHttpHandler (для .NET Core)
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

var botClient = new TelegramBotClient(token, httpClient);
var commandExecutor = new CommandExecutor();  

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
            await commandExecutor.ExecuteAsync(client, message, cancellationToken);
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
