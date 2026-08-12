using Discord;
using SinokBerezki.Application.Models;
using SinokBerezki.Application.Services;
using SinokBerezki.Core.GameModels;
using SinokBerezki.Infrastructure.Repositories;
using System.Text;

namespace SinokBerezki.Discordy.UI;

public static class MonsterUiFactory
{
    // Исправление: вызываем .Build() для получения Embed из EmbedBuilder

    public static (Embed Embed, MessageComponent Components) BuildIntroGuide(ulong authorId)
    {
        var embed = new EmbedBuilder()
            .WithTitle("📖 Руководство: Арена Монстров")
            .WithColor(Color.Blue)
            .WithDescription(
                "**Мирная фаза**\n" +
                "Каждый день в 12:00 МСК вы можете получать нового случайного монстра (гача). " +
                "Вне боя здоровье и мана восстанавливаются пассивно.\n\n" +
                "**Боевая фаза (PvE)**\n" +
                "Бои проходят пошагово. Каждый ход вы планируете заявку: до 2 основных и 2 дополнительных заклинаний. " +
                "Порядок срабатывания зависит от Инициативы.")
            .Build();

        var components = new ComponentBuilder()
            .WithButton("Войти в игру", $"monster_join_{authorId}", ButtonStyle.Success)
            .Build();

        return (embed, components); // embed уже типа Embed
    }

    public static (Embed Embed, MessageComponent Components) BuildFarmMenu(IReadOnlyList<CreatureModel> monsters, int currentIndex, ulong authorId)
    {
        var monster = monsters[currentIndex];

        var embed = new EmbedBuilder()
            .WithTitle($"🏡 Ферма монстров ({currentIndex + 1} из {monsters.Count})")
            .WithColor(Color.DarkGreen)
            .WithDescription("Переключайте курсор, чтобы посмотреть всех существ в загоне.")
            .AddField($"👾 {monster.Name}",
                $"❤️ HP: {monster.CurrentHp} / {monster.BaseMaxHp}\n" +
                $"💧 MP: {monster.CurrentMp} / {monster.BaseMaxMp}\n" +
                $"⚡ Инициатива: {monster.BaseInitiative}",
                inline: false)
            .Build();

        // Логика зацикливания: если жмем назад на первом - показывает последнего и наоборот
        int prevIndex = currentIndex - 1 < 0 ? monsters.Count - 1 : currentIndex - 1;
        int nextIndex = (currentIndex + 1) % monsters.Count;

        // Отключаем кнопки переключения, если монстр всего один
        bool onlyOneMonster = monsters.Count == 1;

        var components = new ComponentBuilder()
                    // Добавили _prev в конец ID
                    .WithButton("⬅️ Назад", $"farm_nav_{authorId}_{prevIndex}_prev", ButtonStyle.Secondary, disabled: onlyOneMonster)
                    .WithButton("Выбрать 🐾", $"monster_menu_{authorId}_{monster.Id}", ButtonStyle.Success)
                    // Добавили _next в конец ID
                    .WithButton("Вперед ➡️", $"farm_nav_{authorId}_{nextIndex}_next", ButtonStyle.Secondary, disabled: onlyOneMonster)
                    .WithButton("Приручить", $"monster_gacha_{authorId}", ButtonStyle.Primary, row: 1)
                    .Build();

        return (embed, components); // embed уже типа Embed
    }

    public static (Embed Embed, MessageComponent Components) BuildMonsterMenu(CreatureModel monster, ulong authorId, IStaticDataCatalog catalog)
    {
        var embedBuilder = new EmbedBuilder()
            .WithTitle($"👾 {monster.Name}")
            .WithColor(Color.Green)
            .WithDescription("Главное меню управления существом.")
            .AddField("❤️ HP", $"{monster.CurrentHp} / {monster.BaseMaxHp}", inline: true)
            .AddField("💧 MP", $"{monster.CurrentMp} / {monster.BaseMaxMp}", inline: true)
            .AddField("⚡ Инициатива", monster.BaseInitiative.ToString(), inline: true);

        // Ищем врождённую способность
        var abilityGene = monster.Genes.FirstOrDefault(g => g.Category == GeneCategory.Ability);
        if (abilityGene != null && !string.IsNullOrEmpty(abilityGene.AbilityId))
        {
            var ability = catalog.Abilities.FirstOrDefault(a => a.Id == abilityGene.AbilityId);
            if (ability != null)
            {
                embedBuilder.AddField("🌟 Способность", $"**{ability.Name}** (Сила: {abilityGene.Power})\n*{ability.Description}*", inline: false);
            }
        }

        // Ищем заклинания
        var spellGenes = monster.Genes.Where(g => g.Category == GeneCategory.Spell && !string.IsNullOrEmpty(g.SpellArchetypeId));
        var spellsSb = new StringBuilder();
        foreach (var gene in spellGenes)
        {
            var spellIds = gene.SpellArchetypeId!.Split(',');
            foreach (var spellId in spellIds)
            {
                var spell = catalog.Spells.FirstOrDefault(s => s.Id == spellId);
                if (spell != null)
                {
                    spellsSb.AppendLine($"🔹 **{spell.Name}** ({spell.ManaCost} MP) — {spell.Description}");
                }
            }
        }

        if (spellsSb.Length > 0)
        {
            embedBuilder.AddField("📜 Заклинания", spellsSb.ToString(), inline: false);
        }
        else
        {
            embedBuilder.AddField("📜 Заклинания", "Нет активных заклинаний", inline: false);
        }

        var components = new ComponentBuilder()
            .WithButton("⚔️ Бой", $"monster_battle_{authorId}_{monster.Id}", ButtonStyle.Success)
            .WithButton("🧬 Гены", $"monster_genes_{authorId}_{monster.Id}", ButtonStyle.Secondary)
            // Кнопку "Заклинания" можно убрать, так как они теперь на главном экране
            .WithButton("🔙 На ферму", $"farm_nav_{authorId}_0_prev", ButtonStyle.Primary, row: 1)
            .WithButton("🍃 Отпустить", $"monster_release_{authorId}_{monster.Id}", ButtonStyle.Danger, row: 1)
            .Build();

        return (embedBuilder.Build(), components); // ВАЖНО: вызываем .Build() для получения Embed
    }

    // НОВЫЙ МЕТОД: Окно подтверждения
    public static (Embed Embed, MessageComponent Components) BuildReleaseConfirmation(CreatureModel monster, ulong authorId)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"⚠️ Отпустить {monster.Name}?")
            .WithColor(Color.Red)
            .WithDescription("Вы уверены, что хотите отпустить этого монстра на волю? **Это действие необратимо!**")
            .Build();

        var components = new ComponentBuilder()
            .WithButton("❌ Отмена", $"monster_menu_{authorId}_{monster.Id}", ButtonStyle.Secondary)
            .WithButton("✅ Да, отпустить", $"monster_confirmrelease_{authorId}_{monster.Id}", ButtonStyle.Danger)
            .Build();

        return (embed, components); // embed уже типа Embed
    }

    public static Embed BuildGenesEmbed(CreatureModel monster, IStaticDataCatalog catalog)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"🧬 Гены: {monster.Name}")
            .WithColor(Color.DarkPurple);

        foreach (var group in monster.Genes.GroupBy(g => g.Category))
        {
            string categoryName = group.Key switch
            {
                GeneCategory.Hp => "Слот Здоровья (HP)",
                GeneCategory.Mp => "Слот Маны (MP)",
                GeneCategory.Ability => "Врождённые способности",
                GeneCategory.Spell => "Заклинания",
                _ => "Неизвестно"
            };

            var sb = new StringBuilder();
            foreach (var g in group)
            {
                string typeIcon = g.Type switch { GeneType.Active => "🟢 (A)", GeneType.Stabilizing => "🟡 (S)", _ => "⚪ (X)" };

                if (g.Category == GeneCategory.Ability && !string.IsNullOrEmpty(g.AbilityId))
                {
                    var ability = catalog.Abilities.FirstOrDefault(a => a.Id == g.AbilityId);
                    sb.AppendLine($"`{typeIcon} Сила: {g.Power}` — **{ability?.Name ?? "???"}**");
                }
                else if (g.Category == GeneCategory.Spell)
                {
                    sb.AppendLine($"`{typeIcon} Стихия: {g.Element}`");
                }
                else
                {
                    sb.AppendLine($"`{typeIcon} Сила: {g.Power}`");
                }
            }

            embed.AddField(categoryName, sb.ToString(), inline: false);
        }

        return embed.Build();
    }

    public static Embed BuildSpellsEmbed(CreatureModel monster, IStaticDataCatalog catalog)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"📜 Заклинания: {monster.Name}")
            .WithColor(Color.Blue);

        var spellGenes = monster.Genes.Where(g => g.Category == GeneCategory.Spell && !string.IsNullOrEmpty(g.SpellArchetypeId));
        bool hasSpells = false;

        foreach (var gene in spellGenes)
        {
            var spellIds = gene.SpellArchetypeId!.Split(',');
            foreach (var spellId in spellIds)
            {
                var spell = catalog.Spells.FirstOrDefault(s => s.Id == spellId);
                if (spell != null)
                {
                    hasSpells = true;
                    embed.AddField(
                        $"{spell.Name} ({spell.ManaCost} MP)",
                        $"**Условие:** {spell.Order} | **Тип:** {spell.Type} | **Стихия:** {spell.Element}\n*{spell.Description}*",
                        inline: false);
                }
            }
        }

        if (!hasSpells) embed.WithDescription("У этого существа нет активных заклинаний.");

        return embed.Build();
    }

    public static Embed BuildHelpEmbed(IReadOnlyList<CommandInfo> commands)
    {
        var embed = new EmbedBuilder()
            .WithTitle("📜 Список доступных команд")
            .WithColor(Color.Blue)
            .WithFooter("Префикс: ?");

        if (commands.Count == 0)
        {
            embed.WithDescription("Команды не найдены.");
        }
        else
        {
            foreach (var cmd in commands)
            {
                embed.AddField($"`{cmd.Name}`", cmd.Description ?? "Описание отсутствует", inline: false);
            }
        }

        return embed.Build();
    }

    public static Embed BuildRollEmbed(string formula, int[] rolls, int modifier, int total, string authorName)
    {
        var embed = new EmbedBuilder()
            .WithTitle("🎲 Бросок кубиков")
            .WithColor(Color.Green)
            .WithDescription(formula)
            .WithFooter(authorName);

        int diceCount = rolls.Length;

        if (diceCount <= 50)
        {
            embed.AddField("Броски", string.Join(", ", rolls), inline: false);
        }
        else
        {
            embed.AddField("Броски", "Слишком много кубиков для отображения", inline: false);
        }

        int naturalSum = rolls.Sum();

        if (modifier != 0)
        {
            embed.AddField("Сумма бросков", naturalSum.ToString(), inline: true);
            embed.AddField("Модификатор", (modifier > 0 ? "+" : "") + modifier, inline: true);
            embed.AddField("Итог", total.ToString(), inline: false);
        }
        else
        {
            embed.AddField("Сумма", total.ToString(), inline: false);
        }

        return embed.Build();
    }
}