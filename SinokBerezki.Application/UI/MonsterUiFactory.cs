using Discord;
using SinokBerezki.Core.GameModels;
using System.Text;
using SinokBerezki.Infrastructure.Repositories;

namespace SinokBerezki.Application.UI;

public static class MonsterUiFactory
{
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

        return (embed, components);
    }

    public static (Embed Embed, MessageComponent Components) BuildMonsterMenu(CreatureModel monster, ulong authorId)
    {
        var embed = new EmbedBuilder()
            .WithTitle($"👾 {monster.Name}")
            .WithColor(Color.Green)
            .WithDescription("Главное меню управления существом.")
            .AddField("❤️ HP", $"{monster.CurrentHp} / {monster.BaseMaxHp}", inline: true)
            .AddField("💧 MP", $"{monster.CurrentMp} / {monster.BaseMaxMp}", inline: true)
            .AddField("⚡ Инициатива", monster.BaseInitiative.ToString(), inline: true)
            .Build();

        var components = new ComponentBuilder()
            .WithButton("Бой", $"monster_battle_{authorId}", ButtonStyle.Danger)
            .WithButton("Приручить", $"monster_gacha_{authorId}", ButtonStyle.Primary)
            .WithButton("Гены", $"monster_genes_{authorId}_{monster.Id}", ButtonStyle.Secondary)
            .WithButton("Заклинания", $"monster_spells_{authorId}_{monster.Id}", ButtonStyle.Secondary)
            .Build();

        return (embed, components);
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
                    sb.AppendLine($"`{typeIcon} Стихия: {g.Element}` — Книга заклинаний");
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

        if (!hasSpells)
        {
            embed.WithDescription("У этого существа нет активных заклинаний.");
        }

        return embed.Build();
    }
}