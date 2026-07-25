using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
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
        
        using var stream = System.IO.File.OpenRead("images/greetings.jpg");

        if(!await _userRepo.ExistsAsync(message.From.Id))
        {
            await botClient.SendPhotoAsync(
                chatId: message.Chat.Id,
                photo: new InputFileStream(stream), 
                caption: """
                Приветствуем вас в агенстве Домашка от Тайлера!
                
                
                Нажимая кнопку "Стать агентом", вы подтвреждаете, что не будете:
                1) Вести свою команду к поражению
                2) Выполнять задания в рейтинговых режимах
                """,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            var agent = await _userRepo.GetByTelegramIdAsync(message.From.Id);

            var stream1 = System.IO.File.OpenRead("images/menu.jpg");
            await botClient.SendPhotoAsync(
                message.Chat.Id,
                photo: new InputFileStream(stream1),
                caption: $"⛑︎ Здравия желаю, агент {agent.AgentName}!",
                replyMarkup: GetMainMenuKeyboard(),
                cancellationToken: cancellationToken);
        }
    }
    private InlineKeyboardMarkup GetMainMenuKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("⛑︎ Профиль", "profile") },
            new[] { InlineKeyboardButton.WithCallbackData("✉︎ Задания", "getMode") }
        });
    }

}