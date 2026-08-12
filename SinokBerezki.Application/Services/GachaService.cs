using SinokBerezki.Core.GameModels;
using SinokBerezki.Infrastructure.Repositories;

namespace SinokBerezki.Application.Services;

public enum GachaResultType
{
    Success,
    PoolIsFull,      // Достигнут лимит в 10 монстров
    AlreadyClaimedToday // Выдача сегодня в 12:00 МСК уже забиралась
}

public record GachaResult(GachaResultType Type, CreatureModel? CreatedCreature, string Message);

public class GachaService
{
    private readonly IPlayerRepository _repository;
    private readonly CreatureGenerator _generator;

    public GachaService(IPlayerRepository repository, CreatureGenerator generator)
    {
        _repository = repository;
        _generator = generator;
    }

    public async Task<GachaResult> TryClaimDailyMonsterAsync(ulong userId)
    {
        var profile = await _repository.GetOrCreateProfileAsync(userId);

        if (profile.Monsters.Count >= 10)
        {
            return new GachaResult(GachaResultType.PoolIsFull, null, "❌ Твой загон переполнен (максимум 10 монстров). Отпусти кого-то, чтобы получить нового!");
        }

        // Вычисление "Текущего дня Гачи" по МСК с учетом отсечки в 12:00
        DateTime mskNow = DateTime.UtcNow.AddHours(3);

        // Если текущее время меньше 12:00 МСК, значит «день гачи» отсчитывается с вчерашних 12:00
        DateTime currentGachaCycleDate = mskNow.Hour < 12
            ? mskNow.Date.AddDays(-1)
            : mskNow.Date;

        string currentCycleKey = currentGachaCycleDate.ToString("yyyy-MM-dd");

        // Проверяем, новичок ли это, по отсутствию записи о прошлой гаче (а не по количеству монстров)
        bool isFirstTime = string.IsNullOrEmpty(profile.LastGachaDateMsk);

        if (!isFirstTime && profile.LastGachaDateMsk == currentCycleKey)
        {
            DateTime nextReset = currentGachaCycleDate.AddDays(1).AddHours(12);
            TimeSpan timeRemaining = nextReset - mskNow;
            return new GachaResult(GachaResultType.AlreadyClaimedToday, null, $"⏳ Следующий призыв будет доступен в 12:00 МСК (через {timeRemaining.Hours}ч {timeRemaining.Minutes}мин).");
        }

        // Генерация нового монстра
        var monster = _generator.GenerateRandomCreature(userId);
        profile.Monsters.Add(monster);

        string message;

        if (isFirstTime)
        {
            // Отмечаем, что стартовый бонус получен, но НЕ записываем текущий день.
            // Это позволит игроку сразу после стартового монстра использовать ежедневный призыв!
            profile.LastGachaDateMsk = "Starter_Claimed";
            message = "🎉 Добро пожаловать в игру! Ты получил своего стартового монстра! Твоя ежедневная попытка призыва всё ещё доступна.";
        }
        else
        {
            // Обновляем дату только для обычных ежедневных призывов
            profile.LastGachaDateMsk = currentCycleKey;
            message = "✨ Призыв успешен! Новый монстр добавлен в твой пул.";
        }

        await _repository.SaveProfileAsync(profile);

        return new GachaResult(GachaResultType.Success, monster, message);
    }
}