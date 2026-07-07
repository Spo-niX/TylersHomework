using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TylersHomework.Attributes;

namespace TylersHomework.Commands;

[Command("/start")]
public class StartCommand
{
    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Стать агентом", "createAgent"),
            }
        });

        await botClient.SendTextMessageAsync(
            chatId: message.Chat.Id,
            text: "Ну хули ты лысый плаки плаки нажимая кнопку ты подтверждаешь что не будешь руинить туда сюда",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }
}