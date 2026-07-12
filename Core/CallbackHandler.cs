// ========== CallbackHandler.cs ==========
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TylersHomework.Core.Database.Repositories;

namespace TylersHomework.Core;

public class CallbackHandlerHelp
{
    private readonly UserRepository _userRepo;

    public CallbackHandlerHelp(UserRepository userRepo)
    {
        _userRepo = userRepo;
    }
    public async Task HandleHelp(ITelegramBotClient bot, CallbackQuery callback, CancellationToken ct)
    {
        var chatId = callback.Message!.Chat.Id;
        var messageId = callback.Message.MessageId;

        await bot.AnswerCallbackQueryAsync(
            callbackQueryId: callback.Id,
            cancellationToken: ct
        );
        bool isEx = false;
        var agent = new Database.Models.User();
        if(await _userRepo.ExistsAsync(callback.From.Id))
        {
            agent = await _userRepo.GetByTelegramIdAsync(callback.From.Id);
            isEx = true;
        }

        switch (callback.Data)
        {
            case "menu":
                if(!isEx)
                {
                    await bot.EditMessageTextAsync(
                        chatId, messageId,
                        "Доступ в этот отдел запрещён для простых смертных. Напишите /start, чтобы стать агентом",
                        cancellationToken: ct
                    );
                    break;
                }
                await bot.EditMessageTextAsync(
                        chatId, messageId,
                        $"Здравия желаю, агент {agent.AgentName}!",
                        replyMarkup: GetMainMenuKeyboard(),
                        cancellationToken: ct
                    );
                break;
            case "setName":
                UserStates.SetState(callback.From.Id, "waitName");
        
                await bot.EditMessageTextAsync(
                    chatId, messageId,
                    "Введите ваш позывной (от 3 до 10 символов, только буквы):",
                    cancellationToken: ct
                );
                break;
            case "profile":
                if(!isEx)
                {
                    await bot.EditMessageTextAsync(
                        chatId, messageId,
                        "Доступ в этот отдел запрещён для простых смертных. Напишите /start, чтобы стать агентом",
                        cancellationToken: ct
                    );
                    break;
                }
                await bot.EditMessageTextAsync(
                    chatId, messageId,
                    $"""
                    Здравия желаю, агент {agent.AgentName}!

                    Ваше актуальное значение MMR {agent.Mmr},
                    что даёт вам право на титул {getRang(agent.Mmr)}.

                    На данный момент, вы выполнили {agent.TaskCompleted} заданий!
                    """,
                    replyMarkup: GetBackButton("menu"),
                    cancellationToken: ct
                );

                break;

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

    private InlineKeyboardMarkup GetBackButton(string target)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Назад", target) }
        });
    }

    private string getRang(int mmr)
    {
        if(mmr < 100)
        {
            return "Новобранец";
        }
        else if(mmr < 200)
        {
            return "Агент класса Г";
        }
        else if(mmr < 400)
        {
            return "Водонос";
        }
        else if(mmr < 600)
        {
            return "Навозный пёс";
        }
        else if(mmr < 1000)
        {
            return "Уличный решала";
        }
        else if(mmr < 1400)
        {
            return "Проверенный волк";
        }
        else if(mmr < 1700)
        {
            return "Страх turbo";
        }
        else if(mmr < 2000)
        {
            return "Звезда LP";
        }
        else if(mmr < 2400)
        {
            return "Мастер билдостроения";
        }
        else if(mmr < 2800)
        {
            return "Агент старой закалки";
        }
        else if(mmr < 3200)
        {
            return "All pick вождь";
        }
        else
        {
            return "Убийца нубов";
        }
    }
}