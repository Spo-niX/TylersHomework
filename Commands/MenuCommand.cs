using Newtonsoft.Json.Serialization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TylersHomework.Attributes;
using TylersHomework.Core.Database.Repositories;

namespace TylersHomework.Commands;

[Command("/menu")]
public class MenuCommand
{
    private readonly UserRepository _userRepo;
    public MenuCommand(UserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task ExecuteAsync(ITelegramBotClient botClient, Message message, CancellationToken cancellationToken)
    {
        if(await _userRepo.ExistsAsync(message.From.Id))
        {
            var agent = await _userRepo.GetByTelegramIdAsync(message.From.Id);   

            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                $"Здравия желаю, агент {agent.AgentName}!",
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: cancellationToken);
        }
        else
        {
            await botClient.SendTextMessageAsync(
                message.Chat.Id,
                "Доступ в этот отдел запрещён для простых смертных. Напишите /start, чтобы стать агентом",
                cancellationToken: cancellationToken);
        }
    }

    private InlineKeyboardMarkup GetMainMenuKeyboard()
        {
            return new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Профиль", "profile") },
                new[] { InlineKeyboardButton.WithCallbackData("Задания", "settings") }
            });
        }
}
