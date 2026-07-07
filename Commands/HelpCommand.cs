using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TylersHomework.Attributes;

namespace TylersHomework.Commands;

[Command("/help")]
public class HelpCommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var helpText = """
                       📚 Доступные команды:
                       /start - Приветствие
                       /help - Эта справка
                       """;
        
        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: helpText,
            cancellationToken: cancellationToken
        );
    }
}
