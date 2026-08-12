namespace SinokBerezki.Core.GameModels;

public class CreatureModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ulong OwnerId { get; set; }
    public string Name { get; set; } = "Безымянный монстр";

    public List<GeneModel> Genes { get; set; } = new(12);

    public int CurrentHp { get; set; }
    public int CurrentMp { get; set; }

    // Вместо DateTime храним номер глобального тика (1 тик = 5 минут)
    public long LastProcessedTick { get; set; }

    public CreatureModel()
    {
        // При создании существа фиксируем текущий глобальный тик
        LastProcessedTick = GetCurrentGlobalTick();
    }

    public int BaseMaxHp => CalculateSlotStat(GeneCategory.Hp, 5);
    public int BaseMaxMp => CalculateSlotStat(GeneCategory.Mp, 5);
    public int BaseInitiative => Genes.Where(g => g.Type == GeneType.Junk).Sum(g => g.Power);

    private int CalculateSlotStat(GeneCategory category, int baseValue)
    {
        var slotGenes = Genes.Where(g => g.Category == category).ToList();
        int activeSum = slotGenes.Where(g => g.Type == GeneType.Active).Sum(g => g.Power);

        var stabilizingGenes = slotGenes.Where(g => g.Type == GeneType.Stabilizing).ToList();
        int stabilizingSum = stabilizingGenes.Count >= 2 ? stabilizingGenes.Sum(g => g.Power) : 0;

        return baseValue + activeSum + stabilizingSum;
    }

    // Вспомогательный метод для получения текущего тика по UTC
    private static long GetCurrentGlobalTick()
    {
        // ToUnixTimeSeconds() всегда возвращает время по Гринвичу (UTC)
        // Делим на 300 секунд (5 минут), чтобы получить номер 5-минутного интервала
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 300;
    }

    // Вызываем этот метод каждый раз перед тем, как работать с HP/MP монстра
    public void ApplyOfflineRegen()
    {
        long currentTick = GetCurrentGlobalTick();
        long ticksPassed = currentTick - LastProcessedTick;

        if (ticksPassed > 0)
        {
            CurrentHp = (int)Math.Min(CurrentHp + ticksPassed, BaseMaxHp);
            CurrentMp = (int)Math.Min(CurrentMp + ticksPassed, BaseMaxMp);

            // Запоминаем тик, на котором остановились
            LastProcessedTick = currentTick;
        }
    }
}