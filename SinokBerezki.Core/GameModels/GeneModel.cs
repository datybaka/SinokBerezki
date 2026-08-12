namespace SinokBerezki.Core.GameModels;

public class GeneModel
{
    public GeneCategory Category { get; set; }
    public GeneType Type { get; set; }
    public int Power { get; set; } // Значение от 1 до 6

    // Опциональные поля, которые заполняются при генерации
    public string? AbilityId { get; set; } // Идентификатор врожденной способности
    public string? SpellArchetypeId { get; set; } // Идентификатор архетипа заклинания
    public ElementType Element { get; set; } = ElementType.None;
}

public enum GeneCategory
{
    Hp,
    Mp,
    Ability, // Врождённая способность (ВС)
    Spell    // Заклинание (ЗАКЛ)
}

public enum GeneType
{
    Active,      // (A) Работает всегда
    Stabilizing, // (S) Работает только если их >= 2 в слоте
    Junk         // (X) Дает инициативу, не дает статов слота
}

public enum ElementType
{
    None,
    Fire,
    Water,
    Frost,
    Electric,
    Kinetic,
    Necrotic,
    Life,
    Chaos
}