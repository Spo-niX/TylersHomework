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

var builder = Host.CreateApplicationBuilder();

string token = builder.Configuration["token"]!; 

var dbPath = "bot.db";
DatabaseConnection.Initialize(dbPath);

builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<CallbackHandlerHelp>();

var ServiceProvider = builder.Services.BuildServiceProvider();
var CallbackHandler = ServiceProvider.GetRequiredService<CallbackHandlerHelp>();

var botClient = new TelegramBotClient(token);
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
