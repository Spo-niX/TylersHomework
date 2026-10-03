using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TylersHomework.Core;
using TylersHomework.Core.Database.Repositories;

public class BotService
{
    private readonly ITelegramBotClient _bot;              // singleton
    private readonly CallbackHandlerHelp _callbackHandler; // сервис
    private readonly HttpClient _httpClient;               // singleton
    private readonly CommandExecutor _commandExecutor;     // сервис
    private readonly UserRepository _userRepo;             // сервис
    private readonly UserTaskRepository _taskRepo;         // сервис

    public BotService(
        ITelegramBotClient bot,
        CallbackHandlerHelp callbackHandler,
        HttpClient httpClient,
        CommandExecutor commandExecutor,
        UserRepository userRepo,
        UserTaskRepository taskRepo)
    {
        _bot = bot;
        _callbackHandler = callbackHandler;
        _httpClient = httpClient;
        _commandExecutor = commandExecutor;
        _userRepo = userRepo;
        _taskRepo = taskRepo;
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _bot.StartReceiving(
            updateHandler: HandleUpdateAsync,     // ← свой метод
            pollingErrorHandler: HandleErrorAsync,
            receiverOptions: new Telegram.Bot.Polling.ReceiverOptions { AllowedUpdates = Array.Empty<UpdateType>() },
            cancellationToken: ct);
        return Task.CompletedTask;
    }

    // Вот сюда приходит Update — как АРГУМЕНТ, не как поле
    private async Task HandleUpdateAsync(
        ITelegramBotClient client,
        Update update,
        CancellationToken ct)
    {
        var handler = new UpdateHandler(
            client, update, ct,
            _callbackHandler, _httpClient, _commandExecutor, _userRepo, _taskRepo);

        await handler.HandleUpdate(client, update, ct);
    }

    private Task HandleErrorAsync(ITelegramBotClient client, Exception ex, CancellationToken ct)
    {
        Console.WriteLine($"ошибка: {ex.Message}");
        return Task.CompletedTask;
    }
}