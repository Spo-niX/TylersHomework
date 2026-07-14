// ========== CallbackHandler.cs ==========
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TylersHomework.Core.Database.Models;
using TylersHomework.Core.Database.Repositories;

namespace TylersHomework.Core;

public class CallbackHandlerHelp
{
    private readonly UserRepository _userRepo;
    private static readonly Dictionary<int, string> HeroNames = new Dictionary<int, string>
    {
        {1, "Anti-Mage"}, {2, "Axe"}, {3, "Bane"}, {4, "Bloodseeker"},
        {5, "Crystal Maiden"}, {6, "Drow Ranger"}, {7, "Earthshaker"},
        {8, "Juggernaut"}, {9, "Mirana"}, {10, "Morphling"},
        {11, "Shadow Fiend"}, {12, "Phantom Lancer"}, {13, "Puck"},
        {14, "Pudge"}, {15, "Razor"}, {16, "Sand King"},
        {17, "Storm Spirit"}, {18, "Sven"}, {19, "Tiny"},
        {20, "Vengeful Spirit"}, {21, "Windranger"}, {22, "Zeus"},
        {23, "Kunkka"}, {25, "Lina"}, {26, "Lion"},
        {27, "Shadow Shaman"}, {28, "Slardar"}, {29, "Tidehunter"},
        {30, "Witch Doctor"}, {31, "Lich"}, {32, "Riki"},
        {33, "Enigma"}, {34, "Tinker"}, {35, "Sniper"},
        {36, "Necrophos"}, {37, "Warlock"}, {38, "Beastmaster"},
        {39, "Queen of Pain"}, {40, "Venomancer"}, {41, "Faceless Void"},
        {42, "Wraith King"}, {43, "Death Prophet"}, {44, "Phantom Assassin"},
        {45, "Pugna"}, {46, "Templar Assassin"}, {47, "Viper"},
        {48, "Luna"}, {49, "Dragon Knight"}, {50, "Dazzle"},
        {51, "Clockwerk"}, {52, "Leshrac"}, {53, "Nature's Prophet"},
        {54, "Lifestealer"}, {55, "Dark Seer"}, {56, "Clinkz"},
        {57, "Omniknight"}, {58, "Enchantress"}, {59, "Huskar"},
        {60, "Night Stalker"}, {61, "Broodmother"}, {62, "Bounty Hunter"},
        {63, "Weaver"}, {64, "Jakiro"}, {65, "Batrider"},
        {66, "Chen"}, {67, "Spectre"}, {68, "Ancient Apparition"},
        {69, "Doom"}, {70, "Ursa"}, {71, "Spirit Breaker"},
        {72, "Gyrocopter"}, {73, "Alchemist"}, {74, "Invoker"},
        {75, "Silencer"}, {76, "Outworld Destroyer"}, {77, "Lycan"},
        {78, "Brewmaster"}, {79, "Shadow Demon"}, {80, "Lone Druid"},
        {81, "Chaos Knight"}, {82, "Meepo"}, {83, "Treant Protector"},
        {84, "Ogre Magi"}, {85, "Undying"}, {86, "Rubick"},
        {87, "Disruptor"}, {88, "Nyx Assassin"}, {89, "Naga Siren"},
        {90, "Keeper of the Light"}, {91, "Io"}, {92, "Visage"},
        {93, "Slark"}, {94, "Medusa"}, {95, "Troll Warlord"},
        {96, "Centaur Warrunner"}, {97, "Magnus"}, {98, "Timbersaw"},
        {99, "Bristleback"}, {100, "Tusk"}, {101, "Skywrath Mage"},
        {102, "Abaddon"}, {103, "Elder Titan"}, {104, "Legion Commander"},
        {105, "Techies"}, {106, "Ember Spirit"}, {107, "Earth Spirit"},
        {108, "Underlord"}, {109, "Terrorblade"}, {110, "Phoenix"},
        {111, "Oracle"}, {112, "Winter Wyvern"}, {113, "Arc Warden"},
        {114, "Monkey King"}, {119, "Dark Willow"}, {120, "Pangolier"},
        {121, "Grimstroke"}, {123, "Hoodwink"}, {126, "Void Spirit"},
        {128, "Snapfire"}, {129, "Mars"}, {131, "Ringmaster"},
        {135, "Dawnbreaker"}, {136, "Marci"}, {137, "Primal Beast"},
        {138, "Muerta"}, {145, "Kez"}, {155, "Largo"}
    };

    private static readonly Dictionary<int, string> sortedHeroNames = new Dictionary<int, string>
    {
        {1, "Anti-Mage"}, {2, "Axe"}, {3, "Bane"}, {4, "Bloodseeker"},
        {5, "Crystal Maiden"}, {6, "Drow Ranger"}, {7, "Earthshaker"},
        {8, "Juggernaut"}, {9, "Mirana"}, {10, "Shadow Fiend"},
        {11, "Morphling"}, {12, "Phantom Lancer"}, {13, "Puck"}, {14, "Pudge"},
        {15, "Razor"}, {16, "Sand King"}, {17, "Storm Spirit"}, {18, "Sven"},
        {19, "Tiny"}, {20, "Vengeful Spirit"}, {21, "Windranger"}, {22, "Zeus"},
        {23, "Kunkka"}, {24, "Lina"}, {25, "Lich"}, {26, "Lion"},
        {27, "Shadow Shaman"}, {28, "Slardar"}, {29, "Tidehunter"},
        {30, "Witch Doctor"}, {31, "Riki"}, {32, "Enigma"}, {33, "Tinker"},
        {34, "Sniper"}, {35, "Necrophos"}, {36, "Warlock"}, {37, "Beastmaster"},
        {38, "Queen of Pain"}, {39, "Venomancer"}, {40, "Faceless Void"},
        {41, "Wraith King"}, {42, "Death Prophet"}, {43, "Phantom Assassin"},
        {44, "Pugna"}, {45, "Templar Assassin"}, {46, "Viper"}, {47, "Luna"},
        {48, "Dragon Knight"}, {49, "Dazzle"}, {50, "Clockwerk"}, {51, "Leshrac"},
        {52, "Nature's Prophet"}, {53, "Lifestealer"}, {54, "Dark Seer"},
        {55, "Clinkz"}, {56, "Omniknight"}, {57, "Enchantress"}, {58, "Huskar"},
        {59, "Night Stalker"}, {60, "Broodmother"}, {61, "Bounty Hunter"},
        {62, "Weaver"}, {63, "Jakiro"}, {64, "Batrider"}, {65, "Chen"},
        {66, "Spectre"}, {67, "Doom"}, {68, "Ancient Apparition"}, {69, "Ursa"},
        {70, "Spirit Breaker"}, {71, "Gyrocopter"}, {72, "Alchemist"},
        {73, "Invoker"}, {74, "Silencer"}, {75, "Outworld Destroyer"},
        {76, "Lycan"}, {77, "Brewmaster"}, {78, "Shadow Demon"},
        {79, "Lone Druid"}, {80, "Chaos Knight"}, {81, "Meepo"},
        {82, "Treant Protector"}, {83, "Ogre Magi"}, {84, "Undying"},
        {85, "Rubick"}, {86, "Disruptor"}, {87, "Nyx Assassin"},
        {88, "Naga Siren"}, {89, "Keeper of the Light"}, {90, "Io"},
        {91, "Visage"}, {92, "Slark"}, {93, "Medusa"}, {94, "Troll Warlord"},
        {95, "Centaur Warrunner"}, {96, "Magnus"}, {97, "Timbersaw"},
        {98, "Bristleback"}, {99, "Tusk"}, {100, "Skywrath Mage"},
        {101, "Abaddon"}, {102, "Elder Titan"}, {103, "Legion Commander"},
        {104, "Ember Spirit"}, {105, "Earth Spirit"}, {106, "Terrorblade"},
        {107, "Phoenix"}, {108, "Oracle"}, {109, "Techies"},
        {110, "Winter Wyvern"}, {111, "Arc Warden"}, {112, "Abyssal Underlord"},
        {113, "Monkey King"}, {114, "Pangolier"}, {115, "Dark Willow"},
        {116, "Grimstroke"}, {117, "Mars"}, {118, "Void Spirit"},
        {119, "Snapfire"}, {120, "Hoodwink"}, {121, "Dawnbreaker"},
        {122, "Marci"}, {123, "Primal Beast"}, {124, "Muerta"},
        {125, "Ringmaster"}, {126, "Kez"}, {127, "Largo"}
    };

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
        var rnd = new Random();
        var ids = new HashSet<int>();
        var eIds = new List<int>();
        if (await _userRepo.ExistsAsync(callback.From.Id))
        {
            agent = await _userRepo.GetByTelegramIdAsync(callback.From.Id);
            isEx = true;
        }

        switch (callback.Data)
        {
            case "menu":
                if (!isEx)
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
                if (!isEx)
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
            case "getMode":
                if (!isEx)
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
                        "Агент, выберите желаемый режим",
                        replyMarkup: getModeKB(),
                        cancellationToken: ct
                    );
                break;
            case "turbo":
                if (!isEx)
                {
                    await bot.EditMessageTextAsync(
                        chatId, messageId,
                        "Доступ в этот отдел запрещён для простых смертных. Напишите /start, чтобы стать агентом",
                        cancellationToken: ct
                    );
                    break;
                }
                await getStarterTask(ids, eIds, rnd, callback, bot, ct, chatId, messageId, true);
                break;
            case "allpick":
                if (!isEx)
                {
                    await bot.EditMessageTextAsync(
                        chatId, messageId,
                        "Доступ в этот отдел запрещён для простых смертных. Напишите /start, чтобы стать агентом",
                        cancellationToken: ct
                    );
                    break;
                }
                await getStarterTask(ids, eIds, rnd, callback, bot, ct, chatId, messageId, false);
                break;
            case "task1":
                await getTask(callback, bot, ct, chatId, messageId, 0);
                break;
            case "task2":
                await getTask(callback, bot, ct, chatId, messageId, 1);
                break;
            case "task3":
                UserStates.SetState(callback.From.Id, "waitId");
                await getTask(callback, bot, ct, chatId, messageId, 2);
                break;
            case "giveUp":
                agent.Mmr -= 25 + rnd.Next(-5, 6);
                if(agent.Mmr < 0)
                {
                    agent.Mmr = 0; 
                }
                UserTaskState.ClearState(callback.From.Id);
                break;
        }
    }

    private InlineKeyboardMarkup GetMainMenuKeyboard()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Профиль", "profile") },
            new[] { InlineKeyboardButton.WithCallbackData("Задания", "getMode") }
        });
    }

    private InlineKeyboardMarkup getModeKB()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Turbo", "turbo") },
            new[] { InlineKeyboardButton.WithCallbackData("All pick", "allpick") }
        });
    }

    private InlineKeyboardMarkup getTaskKB()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Задние 1", "task1") },
            new[] { InlineKeyboardButton.WithCallbackData("Задание 2", "task2") },
            new[] { InlineKeyboardButton.WithCallbackData("Задание 3", "task3") },
        });
    }

    private InlineKeyboardMarkup getGiveUpKB()
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData("Сдаться", "giveUp") },
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
        if (mmr < 100)
        {
            return "Новобранец";
        }
        else if (mmr < 200)
        {
            return "Агент класса Г";
        }
        else if (mmr < 400)
        {
            return "Водонос";
        }
        else if (mmr < 600)
        {
            return "Навозный пёс";
        }
        else if (mmr < 1000)
        {
            return "Уличный решала";
        }
        else if (mmr < 1400)
        {
            return "Проверенный волк";
        }
        else if (mmr < 1700)
        {
            return "Страх turbo";
        }
        else if (mmr < 2000)
        {
            return "Звезда LP";
        }
        else if (mmr < 2400)
        {
            return "Мастер билдостроения";
        }
        else if (mmr < 2800)
        {
            return "Агент старой закалки";
        }
        else if (mmr < 3200)
        {
            return "All pick вождь";
        }
        else
        {
            return "Убийца нубов";
        }
    }

    private async Task getStarterTask(
        HashSet<int> ids, List<int> eIds, Random rnd, CallbackQuery callback, ITelegramBotClient bot,
        CancellationToken ct, long chatId, int messageId, bool isTurbo)
    {
        while (ids.Count < 3)
        {
            ids.Add(rnd.Next(1, 128));
        }

        eIds = ids.ToList();

        eIds.Append(isTurbo ? 0 : 1);

        UserTaskState.SetState(callback.From.Id, eIds);

        await bot.EditMessageTextAsync(
                chatId, messageId,
                $"""
                        Ваши текущие герои (выбирайте после того, как будете уверены, что они не в бане)
                        
                        1 - {GetHeroNameSorted(eIds[0])}
                        2 - {GetHeroNameSorted(eIds[1])}
                        3 - {GetHeroNameSorted(eIds[2])}
                        """,
                replyMarkup: getTaskKB(),
                cancellationToken: ct
        );
    }

    private async Task getTask(CallbackQuery callback, ITelegramBotClient bot, CancellationToken ct, long chatId, int messageId, byte nmb)
    {
        var st = UserTaskState.GetState(callback.From.Id);

        var task = GetUserTask(st[3] == 0, callback.From.Id, st[nmb]);
        await bot.EditMessageTextAsync(
            chatId, messageId,
            $"""
            Ваше итоговое задание:

            Режим - {(task.Mode == 0 ? "turbo" : "all pick")}
            Герой - {GetHeroNameSorted(task.Hero)}
            Предметы - {task.Slots![0]}, {task.Slots[1]}, {task.Slots[2]}, {task.Slots[3]}, {task.Slots[4]}, {task.Slots[5]}
            Цель - победа, с выполнением заданий

            Отправьте id матча, как только будете готовы, или сдайтесь, и потеряйте MMR
            """,
            replyMarkup: getGiveUpKB(),
            cancellationToken: ct);
    }


    private UserTask GetUserTask(bool isTurbo, long ownerId, int hero)
    {
        var rnd = new Random();

        var slt = new List<string>();
        slt[1] = GetBootsItem(rnd.Next(1, 6));
        for (byte i = 1; i < 6; i++)
        {
            slt.Append(getItem(rnd.Next(0, 68)));
        }
        return new UserTask
        {
            OwnerId = ownerId,
            Hero = hero,
            Mode = isTurbo ? 0 : 1,
            Slots = slt
        };
    }

    private string getItem(int id)
    {
        switch (id)
        {
            case 0: return "blink";
            case 1: return "phase_boots";
            case 2: return "power_treads";
            case 3: return "hand_of_midas";
            case 4: return "sheepstick";
            case 5: return "cyclone";
            case 6: return "force_staff";
            case 7: return "dagon";
            case 8: return "scepter";
            case 9: return "refresher";
            case 10: return "assault";
            case 11: return "heart";
            case 12: return "black_king_bar";
            case 13: return "shivas_guard";
            case 14: return "bloodstone";
            case 15: return "blade_mail";
            case 16: return "rapier";
            case 17: return "monkey_king_bar";
            case 18: return "radiance";
            case 19: return "butterfly";
            case 20: return "greater_crit";
            case 21: return "basher";
            case 22: return "bfury";
            case 23: return "manta";
            case 24: return "armlet";
            case 25: return "invis_sword";
            case 26: return "sange_and_yasha";
            case 27: return "satanic";
            case 28: return "mjollnir";
            case 29: return "skadi";
            case 30: return "desolator";
            case 31: return "mask_of_madness";
            case 32: return "diffusal_blade";
            case 33: return "ethereal_blade";
            case 34: return "arcane_boots";
            case 35: return "rod_of_atos";
            case 36: return "abyssal_blade";
            case 37: return "heavens_halberd";
            case 38: return "tranquil_boots";
            case 39: return "travel_boots_2";
            case 40: return "lotus_orb";
            case 41: return "solar_crest";
            case 42: return "guardian_greaves";
            case 43: return "aether_lens";
            case 44: return "octarine_core";
            case 45: return "dragon_lance";
            case 46: return "iron_talon";
            case 47: return "blight_stone";
            case 48: return "tango_single";
            case 49: return "crimson_guard";
            case 50: return "wind_lace";
            case 51: return "moon_shard";
            case 52: return "silver_edge";
            case 53: return "bloodthorn";
            case 54: return "glimmer_cape";
            case 55: return "hurricane_pike";
            case 56: return "ring_of_basilius";
            case 57: return "urn_of_shadows";
            case 58: return "headdress";
            case 59: return "orchid";
            case 60: return "aegis";
            case 61: return "helm_of_the_dominator";
            case 62: return "maelstrom";
            case 63: return "diffusal_blade_2";
            case 64: return "shadow_amulet";
            case 65: return "vladmir";
            case 66: return "pipe";
            case 67: return "oblivion_staff";
        }
        return null!;
    }

    public string GetBootsItem(int id)
    {
        switch (id)
        {
            case 1: return "phase_boots";
            case 2: return "power_treads";
            case 3: return "arcane_boots";
            case 4: return "tranquil_boots";
            case 5: return "travel_boots_2";
            default: return null!;
        }
    }

    public string GetItemName(int id)
    {
        switch (id)
        {
            case 0: return "empty";
            case 1: return "blink";
            case 2: return "blades_of_attack";
            case 3: return "broadsword";
            case 4: return "chainmail";
            case 5: return "claymore";
            case 6: return "helm_of_iron_will";
            case 7: return "javelin";
            case 8: return "mithril_hammer";
            case 9: return "platemail";
            case 10: return "quarterstaff";
            case 11: return "quelling_blade";
            case 12: return "ring_of_protection";
            case 13: return "gauntlets";
            case 14: return "slippers";
            case 15: return "mantle";
            case 16: return "branches";
            case 17: return "belt_of_strength";
            case 18: return "boots_of_elves";
            case 19: return "robe";
            case 20: return "circlet";
            case 21: return "ogre_axe";
            case 22: return "blade_of_alacrity";
            case 23: return "staff_of_wizardry";
            case 24: return "ultimate_orb";
            case 25: return "gloves";
            case 26: return "lifesteal";
            case 27: return "ring_of_regen";
            case 28: return "sobi_mask";
            case 29: return "boots";
            case 30: return "gem";
            case 31: return "cloak";
            case 32: return "talisman_of_evasion";
            case 33: return "cheese";
            case 34: return "magic_stick";
            case 35: return "recipe_magic_wand";
            case 36: return "magic_wand";
            case 37: return "ghost";
            case 38: return "clarity";
            case 39: return "flask";
            case 40: return "dust";
            case 41: return "bottle";
            case 42: return "ward_observer";
            case 43: return "ward_sentry";
            case 44: return "tango";
            case 45: return "courier";
            case 46: return "tpscroll";
            case 47: return "recipe_travel_boots";
            case 48: return "travel_boots";
            case 49: return "recipe_phase_boots";
            case 50: return "phase_boots";
            case 51: return "demon_edge";
            case 52: return "eagle";
            case 53: return "reaver";
            case 54: return "relic";
            case 55: return "hyperstone";
            case 56: return "ring_of_health";
            case 57: return "void_stone";
            case 58: return "mystic_staff";
            case 59: return "energy_booster";
            case 60: return "point_booster";
            case 61: return "vitality_booster";
            case 62: return "recipe_power_treads";
            case 63: return "power_treads";
            case 64: return "recipe_hand_of_midas";
            case 65: return "hand_of_midas";
            case 66: return "recipe_oblivion_staff";
            case 67: return "oblivion_staff";
            case 68: return "recipe_pers";
            case 69: return "pers";
            case 70: return "recipe_poor_mans_shield";
            case 71: return "poor_mans_shield";
            case 72: return "recipe_bracer";
            case 73: return "bracer";
            case 74: return "recipe_wraith_band";
            case 75: return "wraith_band";
            case 76: return "recipe_null_talisman";
            case 77: return "null_talisman";
            case 78: return "recipe_mekansm";
            case 79: return "mekansm";
            case 80: return "recipe_vladmir";
            case 81: return "vladmir";
            case 84: return "flying_courier";
            case 85: return "recipe_buckler";
            case 86: return "buckler";
            case 87: return "recipe_ring_of_basilius";
            case 88: return "ring_of_basilius";
            case 89: return "recipe_pipe";
            case 90: return "pipe";
            case 91: return "recipe_urn_of_shadows";
            case 92: return "urn_of_shadows";
            case 93: return "recipe_headdress";
            case 94: return "headdress";
            case 95: return "recipe_sheepstick";
            case 96: return "sheepstick";
            case 97: return "recipe_orchid";
            case 98: return "orchid";
            case 99: return "recipe_cyclone";
            case 100: return "cyclone";
            case 101: return "recipe_force_staff";
            case 102: return "force_staff";
            case 103: return "recipe_dagon";
            case 104: return "dagon";
            case 105: return "recipe_necronomicon";
            case 106: return "necronomicon";
            case 107: return "recipe_ultimate_scepter";
            case 108: return "ultimate_scepter";
            case 109: return "recipe_refresher";
            case 110: return "refresher";
            case 111: return "recipe_assault";
            case 112: return "assault";
            case 113: return "recipe_heart";
            case 114: return "heart";
            case 115: return "recipe_black_king_bar";
            case 116: return "black_king_bar";
            case 117: return "aegis";
            case 118: return "recipe_shivas_guard";
            case 119: return "shivas_guard";
            case 120: return "recipe_bloodstone";
            case 121: return "bloodstone";
            case 122: return "recipe_sphere";
            case 123: return "sphere";
            case 124: return "recipe_vanguard";
            case 125: return "vanguard";
            case 126: return "recipe_blade_mail";
            case 127: return "blade_mail";
            case 128: return "recipe_soul_booster";
            case 129: return "soul_booster";
            case 130: return "recipe_hood_of_defiance";
            case 131: return "hood_of_defiance";
            case 132: return "recipe_rapier";
            case 133: return "rapier";
            case 134: return "recipe_monkey_king_bar";
            case 135: return "monkey_king_bar";
            case 136: return "recipe_radiance";
            case 137: return "radiance";
            case 138: return "recipe_butterfly";
            case 139: return "butterfly";
            case 140: return "recipe_greater_crit";
            case 141: return "greater_crit";
            case 142: return "recipe_basher";
            case 143: return "basher";
            case 144: return "recipe_bfury";
            case 145: return "bfury";
            case 146: return "recipe_manta";
            case 147: return "manta";
            case 148: return "recipe_lesser_crit";
            case 149: return "lesser_crit";
            case 150: return "recipe_armlet";
            case 151: return "armlet";
            case 152: return "invis_sword";
            case 153: return "recipe_sange_and_yasha";
            case 154: return "sange_and_yasha";
            case 155: return "recipe_satanic";
            case 156: return "satanic";
            case 157: return "recipe_mjollnir";
            case 158: return "mjollnir";
            case 159: return "recipe_skadi";
            case 160: return "skadi";
            case 161: return "recipe_sange";
            case 162: return "sange";
            case 163: return "recipe_helm_of_the_dominator";
            case 164: return "helm_of_the_dominator";
            case 165: return "recipe_maelstrom";
            case 166: return "maelstrom";
            case 167: return "recipe_desolator";
            case 168: return "desolator";
            case 169: return "recipe_yasha";
            case 170: return "yasha";
            case 171: return "recipe_mask_of_madness";
            case 172: return "mask_of_madness";
            case 173: return "recipe_diffusal_blade";
            case 174: return "diffusal_blade";
            case 175: return "recipe_ethereal_blade";
            case 176: return "ethereal_blade";
            case 177: return "recipe_soul_ring";
            case 178: return "soul_ring";
            case 179: return "recipe_arcane_boots";
            case 180: return "arcane_boots";
            case 181: return "orb_of_venom";
            case 182: return "stout_shield";
            case 183: return "recipe_invis_sword";
            case 184: return "recipe_ancient_janggo";
            case 185: return "ancient_janggo";
            case 186: return "recipe_medallion_of_courage";
            case 187: return "medallion_of_courage";
            case 188: return "smoke_of_deceit";
            case 189: return "recipe_veil_of_discord";
            case 190: return "veil_of_discord";
            case 191: return "recipe_necronomicon_2";
            case 192: return "recipe_necronomicon_3";
            case 193: return "necronomicon_2";
            case 194: return "necronomicon_3";
            case 195: return "recipe_diffusal_blade_2";
            case 196: return "diffusal_blade_2";
            case 197: return "recipe_dagon_2";
            case 198: return "recipe_dagon_3";
            case 199: return "recipe_dagon_4";
            case 200: return "recipe_dagon_5";
            case 201: return "dagon_2";
            case 202: return "dagon_3";
            case 203: return "dagon_4";
            case 204: return "dagon_5";
            case 205: return "recipe_rod_of_atos";
            case 206: return "rod_of_atos";
            case 207: return "recipe_abyssal_blade";
            case 208: return "abyssal_blade";
            case 209: return "recipe_heavens_halberd";
            case 210: return "heavens_halberd";
            case 211: return "recipe_ring_of_aquila";
            case 212: return "ring_of_aquila";
            case 213: return "recipe_tranquil_boots";
            case 214: return "tranquil_boots";
            case 215: return "shadow_amulet";
            case 216: return "enchanted_mango";
            case 218: return "ward_dispenser";
            case 220: return "travel_boots_2";
            case 226: return "lotus_orb";
            case 229: return "solar_crest";
            case 231: return "guardian_greaves";
            case 232: return "aether_lens";
            case 233: return "recipe_aether_lens";
            case 234: return "recipe_dragon_lance";
            case 235: return "octarine_core";
            case 236: return "dragon_lance";
            case 237: return "faerie_fire";
            case 238: return "recipe_iron_talon";
            case 239: return "iron_talon";
            case 240: return "blight_stone";
            case 241: return "tango_single";
            case 242: return "crimson_guard";
            case 244: return "wind_lace";
            case 245: return "recipe_bloodthorn";
            case 247: return "moon_shard";
            case 249: return "silver_edge";
            case 250: return "bloodthorn";
            case 251: return "recipe_echo_sabre";
            case 252: return "echo_sabre";
            case 254: return "glimmer_cape";
            case 257: return "tome_of_knowledge";
            case 262: return "recipe_hurricane_pike";
            case 263: return "hurricane_pike";
            case 265: return "infused_raindrop";
            case 1000: return "halloween_candy_corn";
            case 1001: return "mystery_hook";
            case 1002: return "mystery_arrow";
            case 1003: return "mystery_missile";
            case 1004: return "mystery_toss";
            case 1005: return "mystery_vacuum";
            case 1006: return "halloween_rapier";
            case 1007: return "greevil_whistle";
            case 1008: return "greevil_whistle_toggle";
            case 1009: return "present";
            case 1010: return "winter_stocking";
            case 1011: return "winter_skates";
            case 1012: return "winter_cake";
            case 1013: return "winter_cookie";
            case 1014: return "winter_coco";
            case 1015: return "winter_ham";
            case 1016: return "winter_kringle";
            case 1017: return "winter_mushroom";
            case 1018: return "winter_greevil_treat";
            case 1019: return "winter_greevil_garbage";
            case 1020: return "winter_greevil_chewy";
            default: return "undefined";
        }
    }

    public static string GetHeroNameSorted(int index)
    {
        return sortedHeroNames.TryGetValue(index, out string name) ? name : null!;
    }
}