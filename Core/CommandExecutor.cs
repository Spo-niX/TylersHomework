using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using TylersHomework.Attributes;

namespace TylersHomework.Core;

public class CommandExecutor
{
    private readonly Dictionary<string, Type> _commands = new();
    private readonly IServiceProvider _serviceProvider;
    public CommandExecutor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        // Автоматически находим все классы с атрибутом [Command]
        var assembly = Assembly.GetExecutingAssembly();
        var commandTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<CommandAttribute>() != null);

        foreach (var type in commandTypes)
        {
            var attribute = type.GetCustomAttribute<CommandAttribute>()!;
            _commands[attribute.Name] = type;
            Console.WriteLine($"Загружена команда: {attribute.Name} -> {type.Name}");
        }

        if (_commands.Count == 0)
        {
            Console.WriteLine("⚠️ ВНИМАНИЕ: Команды не найдены! Проверьте атрибуты.");
        }
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(message.Text)) return;

        var commandName = message.Text.Trim();

        Console.WriteLine($"📩 Получена команда: {commandName}");

        if (_commands.TryGetValue(commandName, out var commandType))
        {
            try
            {
                var commandInstance = ActivatorUtilities.CreateInstance(_serviceProvider, commandType);
                
                var method = commandType.GetMethod("ExecuteAsync");
                if (method != null)
                {
                    await (Task)method.Invoke(commandInstance, new object[] { botClient, message, cancellationToken })!;
                }
                else
                {
                    await botClient.SendTextMessageAsync(
                        chatId: message.Chat.Id,
                        text: "⚠️ Ошибка: метод ExecuteAsync не найден в команде.",
                        cancellationToken: cancellationToken
                    );
                }
            }
            catch (Exception ex)
            {
                await botClient.SendTextMessageAsync(
                    chatId: message.Chat.Id,
                    text: $"❌ Ошибка выполнения команды: {ex.Message}",
                    cancellationToken: cancellationToken
                );
            }
        }
    }
}