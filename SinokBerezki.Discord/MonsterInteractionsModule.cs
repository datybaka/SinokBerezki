using Discord;
using Discord.Interactions;
using SinokBerezki.Application.Abstractions;
using SinokBerezki.Application.Commands;
using SinokBerezki.Application.Services;
using SinokBerezki.Core.GameModels;
using SinokBerezki.Infrastructure.Repositories;

namespace SinokBerezki.Discordy;

public class MonsterInteractionsModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IPlayerRepository _repository;
    private readonly GachaService _gachaService;
    private readonly IStaticDataCatalog _catalog;

    public MonsterInteractionsModule(IPlayerRepository repository, GachaService gachaService, IStaticDataCatalog catalog)
    {
        _repository = repository;
        _gachaService = gachaService;
        _catalog = catalog;
    }

    // Обработка кнопки "Войти в игру"
    [ComponentInteraction("btn_enter_game_*")]
    public async Task HandleEnterGame(ulong authorId)
    {
        if (Context.User.Id != authorId)
        {
            await RespondAsync("❌ Это не ваша кнопка!", ephemeral: true);
            return;
        }

        await DeferAsync(); // Отвечаем Discord, что приняли запрос

        var result = await _gachaService.TryClaimDailyMonsterAsync(authorId);

        if (result.Type == GachaResultType.Success && result.CreatedCreature != null)
        {
            await Context.Channel.SendMessageAsync(result.Message);
            // Перерисовываем меню с новым монстром (в реальном проекте используем абстракцию IMessageSender)
            
        }
    }

    // Обработка кнопки "Приручить" (Гача)
    [ComponentInteraction("btn_gacha_*")]
    public async Task HandleGacha(ulong authorId)
    {
        if (Context.User.Id != authorId) return;

        await DeferAsync();
        var result = await _gachaService.TryClaimDailyMonsterAsync(authorId);

        // Отправляем результат (ошибка лимита, кулдауна или успех)
        await FollowupAsync(result.Message, ephemeral: true);
    }

    // Обработка кнопки "Бой" (Болванка)
    [ComponentInteraction("btn_battle_*")]
    public async Task HandleBattle(ulong authorId)
    {
        if (Context.User.Id != authorId) return;

        await RespondAsync("⚔️ Поиск PvE противника... (Функционал в разработке)", ephemeral: true);
    }

    // Обработка кнопки "Гены"
    [ComponentInteraction("btn_genes_*_*")]
    public async Task HandleGenes(string monsterIdStr, ulong authorId)
    {
        if (Context.User.Id != authorId) return;

        var profile = await _repository.GetOrCreateProfileAsync(authorId);
        var monster = profile.Monsters.FirstOrDefault(m => m.Id.ToString() == monsterIdStr);
        if (monster == null) return;

        var embed = new EmbedBuilder()
            .WithTitle("🧬 Гены существа")
            .WithColor(Color.DarkPurple);

        // Группируем гены по слотам для красивого вывода
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

            var lines = group.Select(g =>
            {
                string typePrefix = g.Type switch { GeneType.Active => "🟢 Активный", GeneType.Stabilizing => "🟡 Стабилизирующий", _ => "⚪ Мусорный" };

                if (g.Category == GeneCategory.Ability && g.AbilityId != null)
                {
                    var ability = _catalog.Abilities.FirstOrDefault(a => a.Id == g.AbilityId);
                    return $"- {typePrefix} | Сила: {g.Power} | Способность: **{ability?.Name ?? "Неизвестно"}**";
                }
                if (g.Category == GeneCategory.Spell)
                {
                    return $"- {typePrefix} | Стихия: {g.Element} | Заклинания привязаны";
                }

                return $"- {typePrefix} | Сила: {g.Power}";
            });

            embed.AddField(categoryName, string.Join("\n", lines), inline: false);
        }

        await RespondAsync(embed: embed.Build(), ephemeral: true); // ephemeral означает, что увидит только тот, кто нажал
    }

    // Обработка кнопки "Заклинания"
    [ComponentInteraction("btn_spells_*_*")]
    public async Task HandleSpells(string monsterIdStr, ulong authorId)
    {
        if (Context.User.Id != authorId) return;

        var profile = await _repository.GetOrCreateProfileAsync(authorId);
        var monster = profile.Monsters.FirstOrDefault(m => m.Id.ToString() == monsterIdStr);
        if (monster == null) return;

        var embed = new EmbedBuilder()
            .WithTitle("📜 Книга заклинаний")
            .WithColor(Color.Blue);

        var spellGenes = monster.Genes.Where(g => g.Category == GeneCategory.Spell && !string.IsNullOrEmpty(g.SpellArchetypeId));

        foreach (var gene in spellGenes)
        {
            var spellIds = gene.SpellArchetypeId!.Split(',');
            foreach (var spellId in spellIds)
            {
                var spell = _catalog.Spells.FirstOrDefault(s => s.Id == spellId);
                if (spell != null)
                {
                    embed.AddField(
                        $"{spell.Name} ({spell.ManaCost} MP)",
                        $"**Тип:** {spell.Order}, {spell.Type} | **Стихия:** {spell.Element}\n{spell.Description}",
                        inline: false);
                }
            }
        }

        if (embed.Fields.Count == 0)
        {
            embed.Description = "У этого существа нет активных заклинаний.";
        }

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}