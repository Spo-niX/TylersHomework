using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TylersHomework.Attributes;
using TylersHomework.Core.Database.Repositories;

namespace TylersHomework.Commands;

[Command("/start")]
public class StartCommand
{
    private readonly UserRepository _userRepo;
    public StartCommand(UserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Стать агентом", "setName"),
            }
        });
        if(!await _userRepo.ExistsAsync(message.From.Id))
        {
            await botClient.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: """
                Приветствуем вас в агенстве Домашка от Тайлера!
                
                
                Нажимая кнопку Стать агентом, вы подтвреждаете, что не будете:
                1) Вести свою команду к поражению
                2) Выполнять задания в рейтинговых режимах
                """,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }
    }
}