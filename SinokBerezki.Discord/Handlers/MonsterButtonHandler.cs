using Discord.WebSocket;
using SinokBerezki.Application.Abstractions;
using SinokBerezki.Application.Services;
using SinokBerezki.Discordy.UI;
using SinokBerezki.Infrastructure.Repositories;

namespace SinokBerezki.Discordy.Handlers;

public class MonsterButtonHandler
{
    private readonly IPlayerRepository _repository;
    private readonly GachaService _gachaService;
    private readonly IStaticDataCatalog _catalog;
    private readonly IMessageSender _messageSender;

    public MonsterButtonHandler(
        IPlayerRepository repository,
        GachaService gachaService,
        IStaticDataCatalog catalog,
        IMessageSender messageSender)
    {
        _repository = repository;
        _gachaService = gachaService;
        _catalog = catalog;
        _messageSender = messageSender;
    }

    public async Task HandleButtonAsync(SocketMessageComponent component)
    {
        var customId = component.Data.CustomId;

        // Теперь мы пропускаем и monster_ и farm_
        if (!customId.StartsWith("monster_") && !customId.StartsWith("farm_")) return;

        var parts = customId.Split('_');
        if (parts.Length < 3 || !ulong.TryParse(parts[2], out var ownerId)) return;

        if (component.User.Id != ownerId)
        {
            await component.RespondAsync("❌ Только инициатор может использовать эту кнопку.", ephemeral: true);
            return;
        }

        string prefix = parts[0]; // "monster" или "farm"
        string action = parts[1]; // "nav", "menu", "join", "gacha" и т.д.
        string extraParam = parts.Length > 3 ? parts[3] : string.Empty; // index или monsterId

        if (prefix == "farm" && action == "nav")
        {
            int index = int.TryParse(extraParam, out var i) ? i : 0;
            await HandleFarmNavAsync(component, ownerId, index);
            return;
        }

        if (prefix == "monster")
        {
            switch (action)
            {
                case "menu":
                    await HandleMonsterMenuAsync(component, ownerId, extraParam);
                    break;
                case "join":
                case "gacha":
                    await HandleGachaAsync(component, ownerId);
                    break;
                case "genes":
                    await HandleGenesAsync(component, ownerId, extraParam);
                    break;
                case "release": // Вызов окна подтверждения
                    await HandleReleaseWarningAsync(component, ownerId, extraParam);
                    break;
                case "confirmrelease": // Фактическое удаление
                    await HandleConfirmReleaseAsync(component, ownerId, extraParam);
                    break;
                case "battle":
                    await component.RespondAsync("⚔️ Поиск PvE противника... (Функционал в разработке)", ephemeral: true);
                    break;
            }
        }

        if (!component.HasResponded)
        {
            await component.DeferAsync();
        }
    }

    // НОВЫЙ МЕТОД: Навигация по ферме (вперед/назад/вернуться)
    private async Task HandleFarmNavAsync(SocketMessageComponent component, ulong ownerId, int index)
    {
        var profile = await _repository.GetOrCreateProfileAsync(ownerId);

        if (profile.Monsters.Count == 0)
        {
            await component.RespondAsync("❌ У вас пока нет монстров. Используйте приручение!", ephemeral: true);
            return;
        }

        // Защита от выхода за границы массива (например, если монстра продали/удалили)
        if (index < 0 || index >= profile.Monsters.Count)
        {
            index = 0;
        }

        var (embed, components) = MonsterUiFactory.BuildFarmMenu(profile.Monsters, index, ownerId);

        // UpdateAsync изменяет текущее сообщение, на котором нажали кнопку
        await component.UpdateAsync(msg =>
        {
            msg.Embed = embed;
            msg.Components = components;
        });
    }

    // НОВЫЙ МЕТОД: Переход в меню конкретного монстра при нажатии "Выбрать"
    private async Task HandleMonsterMenuAsync(SocketMessageComponent component, ulong ownerId, string monsterIdStr)
    {
        var profile = await _repository.GetOrCreateProfileAsync(ownerId);
        var monster = profile.Monsters.FirstOrDefault(m => m.Id.ToString() == monsterIdStr);

        if (monster == null)
        {
            await component.RespondAsync("❌ Монстр не найден.", ephemeral: true);
            return;
        }

        var (embed, components) = MonsterUiFactory.BuildMonsterMenu(monster, ownerId, _catalog);

        // UpdateAsync заменяет меню фермы на меню монстра
        await component.UpdateAsync(msg =>
        {
            msg.Embed = embed;
            msg.Components = components;
        });
    }

    private async Task HandleGachaAsync(SocketMessageComponent component, ulong ownerId)
    {
        var result = await _gachaService.TryClaimDailyMonsterAsync(ownerId);
        if (result.Type == GachaResultType.Success && result.CreatedCreature != null)
        {
            await component.RespondAsync(result.Message, ephemeral: true);

            // После успешной гачи отправляем отдельным сообщением меню нового монстра
            // (либо можно обновить текущее сообщение, если хотите, вызвав UpdateAsync)
            await _messageSender.SendMonsterMenuAsync(component.Channel.Id, result.CreatedCreature, ownerId);
        }
        else
        {
            await component.RespondAsync(result.Message, ephemeral: true);
        }
    }

    private async Task HandleGenesAsync(SocketMessageComponent component, ulong ownerId, string monsterIdStr)
    {
        var profile = await _repository.GetOrCreateProfileAsync(ownerId);
        var monster = profile.Monsters.FirstOrDefault(m => m.Id.ToString() == monsterIdStr);
        if (monster == null) return;

        var embed = MonsterUiFactory.BuildGenesEmbed(monster, _catalog);
        await component.RespondAsync(embed: embed, ephemeral: true);
    }

    private async Task HandleSpellsAsync(SocketMessageComponent component, ulong ownerId, string monsterIdStr)
    {
        var profile = await _repository.GetOrCreateProfileAsync(ownerId);
        var monster = profile.Monsters.FirstOrDefault(m => m.Id.ToString() == monsterIdStr);
        if (monster == null) return;

        var embed = MonsterUiFactory.BuildSpellsEmbed(monster, _catalog);
        await component.RespondAsync(embed: embed, ephemeral: true);
    }

    // НОВЫЙ МЕТОД: Окно "Вы уверены?"
    private async Task HandleReleaseWarningAsync(SocketMessageComponent component, ulong ownerId, string monsterIdStr)
    {
        var profile = await _repository.GetOrCreateProfileAsync(ownerId);
        var monster = profile.Monsters.FirstOrDefault(m => m.Id.ToString() == monsterIdStr);

        if (monster == null) return;

        var (embed, components) = MonsterUiFactory.BuildReleaseConfirmation(monster, ownerId);
        await component.UpdateAsync(msg =>
        {
            msg.Embed = embed;
            msg.Components = components;
        });
    }

    // НОВЫЙ МЕТОД: Удаление монстра
    private async Task HandleConfirmReleaseAsync(SocketMessageComponent component, ulong ownerId, string monsterIdStr)
    {
        var profile = await _repository.GetOrCreateProfileAsync(ownerId);
        var monster = profile.Monsters.FirstOrDefault(m => m.Id.ToString() == monsterIdStr);

        if (monster == null)
        {
            await component.RespondAsync("❌ Монстр уже отпущен или не найден.", ephemeral: true);
            return;
        }

        // Удаляем и сохраняем
        profile.Monsters.Remove(monster);
        await _repository.SaveProfileAsync(profile);

        // Если остались ещё монстры — кидаем обратно на ферму
        if (profile.Monsters.Count > 0)
        {
            var (embed, components) = MonsterUiFactory.BuildFarmMenu(profile.Monsters, 0, ownerId);
            await component.UpdateAsync(msg =>
            {
                msg.Embed = embed;
                msg.Components = components;
            });

            // Отправляем тихое уведомление об успехе
            await component.FollowupAsync($"🍃 Вы отпустили **{monster.Name}** на волю.", ephemeral: true);
        }
        else
        {
            // Если монстров больше нет — выводим стартовое окно
            var (embed, components) = MonsterUiFactory.BuildIntroGuide(ownerId);
            await component.UpdateAsync(msg =>
            {
                msg.Embed = embed;
                msg.Components = components;
            });
            await component.FollowupAsync($"🍃 Вы отпустили **{monster.Name}**. Ваш загон теперь пуст!", ephemeral: true);
        }
    }
}