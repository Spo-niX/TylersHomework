// ========== CallbackHandler.cs ==========
using Telegram.Bot;
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
            text: "Не ссы в компот",
            cancellationToken: ct
        );

        switch (callback.Data)
        {
            case "createAgent":
                await bot.EditMessageTextAsync(
                    chatId, messageId,
                    "Главное меню:",
                    replyMarkup: GetMainMenuKeyboard(),
                    cancellationToken: ct
                );
                break;

        }
    }

    private InlineKeyboardMarkup GetMainMenuKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("👤 Профиль", "profile") },
            new[] { InlineKeyboardButton.WithCallbackData("⚙️ Настройки", "settings") }
        });
    }

    private InlineKeyboardMarkup GetBackButton(string target)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("🔙 Назад", target) }
        });
    }
}