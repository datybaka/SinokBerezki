namespace SinokBerezki.Core.GameModels
{
    public record SpellDefinition(
        string Id,
        string Name,
        int ManaCost,
        string Element,
        string Type,
        string Order,
        string Description
    );

    public record AbilityDefinition(
        string Id,
        string Name,
        string Description
    );
}
