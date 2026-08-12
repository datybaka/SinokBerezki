using SinokBerezki.Core.GameModels;
using SinokBerezki.Infrastructure.Repositories;

namespace SinokBerezki.Application.Services;

public class CreatureGenerator
{
    private readonly IStaticDataCatalog _catalog;

    public CreatureGenerator(IStaticDataCatalog catalog)
    {
        _catalog = catalog;
    }

    public CreatureModel GenerateRandomCreature(ulong ownerId)
    {
        var breed = (BreedForCreature)Random.Shared.Next(Enum.GetValues<BreedForCreature>().Length);
        var creature = new CreatureModel
        {
            OwnerId = ownerId,
            Name = $" {breed} Гоша"
        };

        // Случайный баланс генов
        int spellCount = Random.Shared.Next(1, 2);
        int abilityCount = Random.Shared.Next(1, 2);
        int hpCount = 5 + Random.Shared.Next(1, 10 - spellCount - abilityCount);
        int mpCount = 5 + 12 - (hpCount + abilityCount + spellCount);

        AddGenes(creature.Genes, GeneCategory.Hp, hpCount);
        AddGenes(creature.Genes, GeneCategory.Mp, mpCount);
        AddGenes(creature.Genes, GeneCategory.Ability, abilityCount);
        AddGenes(creature.Genes, GeneCategory.Spell, spellCount);

        creature.CurrentHp = creature.BaseMaxHp;
        creature.CurrentMp = creature.BaseMaxMp;

        return creature;
    }

    private void AddGenes(List<GeneModel> list, GeneCategory category, int count)
    {
        for (int i = 0; i < count; i++)
        {
            int roll = Random.Shared.Next(100);
            GeneType type = roll switch
            {
                < 50 => GeneType.Active,
                < 80 => GeneType.Stabilizing,
                _ => GeneType.Junk
            };

            var gene = new GeneModel
            {
                Category = category,
                Type = type,
                Power = Random.Shared.Next(1, 7)
            };

            if (category == GeneCategory.Ability)
            {
                var ability = _catalog.GetRandomAbility();
                gene.AbilityId = ability?.Id;
            }
            else if (category == GeneCategory.Spell)
            {
                gene.Element = (ElementType)Random.Shared.Next(1, 9);
                // Подтягиваем спаренные заклинания для выбранного элемента
                var spells = _catalog.GetRandomSpellsForElement(gene.Element, 2);
                if (spells.Count > 0)
                {
                    gene.SpellArchetypeId = string.Join(",", spells.Select(s => s.Id));
                }
            }

            list.Add(gene);
        }
    }
}

public enum BreedForCreature
{
    Волк,
    Лиса,
    Медведь,
    Орёл,
    Змея,
    Кот,
    Собака,
    Кролик,
    Лошадь,
    Дракон,
    Суслик,
    Хуюслик
}