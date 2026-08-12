using SinokBerezki.Core.GameModels;
using System.Text.Json;

namespace SinokBerezki.Infrastructure.Repositories;

public interface IStaticDataCatalog
{
    IReadOnlyList<SpellDefinition> Spells { get; }
    IReadOnlyList<AbilityDefinition> Abilities { get; }
    AbilityDefinition? GetRandomAbility();
    List<SpellDefinition> GetRandomSpellsForElement(ElementType element, int count = 2);
}

public class StaticDataCatalog : IStaticDataCatalog
{
    public IReadOnlyList<SpellDefinition> Spells { get; private set; } = [];
    public IReadOnlyList<AbilityDefinition> Abilities { get; private set; } = [];

    public StaticDataCatalog(string spellsJsonPath, string abilitiesJsonPath)
    {
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (File.Exists(spellsJsonPath))
        {
            var json = File.ReadAllText(spellsJsonPath);
            Spells = JsonSerializer.Deserialize<List<SpellDefinition>>(json, options) ?? [];
        }

        if (File.Exists(abilitiesJsonPath))
        {
            var json = File.ReadAllText(abilitiesJsonPath);
            Abilities = JsonSerializer.Deserialize<List<AbilityDefinition>>(json, options) ?? [];
        }
    }

    public AbilityDefinition? GetRandomAbility()
    {
        if (Abilities.Count == 0) return null;
        return Abilities[Random.Shared.Next(Abilities.Count)];
    }

    public List<SpellDefinition> GetRandomSpellsForElement(ElementType element, int count = 2)
    {
        string elementStr = element.ToString().ToLower();
        var matching = Spells.Where(s => s.Element.ToLower() == elementStr).ToList();

        // Если для элемента нет заклинаний, берутся случайные из общего пула
        var source = matching.Count >= count ? matching : Spells.ToList();
        return source.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
    }
}