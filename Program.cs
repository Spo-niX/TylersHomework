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


var builder = Host.CreateApplicationBuilder(args);  

//Билдер и его настройки
    
    builder.Configuration.AddEnvironmentVariables();
    builder.Configuration.AddUserSecrets<Program>();


//Прокси настроено для моего сервера
    builder.Services.AddSingleton<HttpClient>(sp =>
    {
        var config = sp.GetRequiredService<IConfiguration>();
        var useProxy = config["useProxy"];
        Console.WriteLine($"[HttpClient] useProxy = '{useProxy ?? "<null>"}'");
        if (config["useProxy"] == "yes")
        {
            var proxy = new WebProxy("socks5://127.0.0.1:1088");

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

            return new HttpClient(socketsHandler);
        }

        return new HttpClient();
    });

//Бд сервисы
    builder.Services.AddSingleton<DatabaseConnection>();
    builder.Services.AddSingleton<UserRepository>();
    builder.Services.AddSingleton<UserTaskRepository>();
    

//Остальные сервисы
    builder.Services.AddSingleton<CallbackHandlerHelp>();
    builder.Services.AddSingleton<CommandExecutor>();
    builder.Services.AddSingleton<BotService>(); 
//Бот
    builder.Services.AddSingleton<ITelegramBotClient>(sp =>
    {
        var httpClient = sp.GetRequiredService<HttpClient>();
        var config = sp.GetRequiredService<IConfiguration>();
        var token = config["token"] ?? throw new InvalidOperationException("Token not found");
        Console.WriteLine(config["token"]);
        return new TelegramBotClient(token, httpClient);
    });
    
//Собираем
    var app = builder.Build();

//Иницилизируем бд
    DatabaseConnection.Initialize("bot.db");

//Запуск
var botService = app.Services.GetRequiredService<BotService>();
await botService.StartAsync();

await app.RunAsync();
    