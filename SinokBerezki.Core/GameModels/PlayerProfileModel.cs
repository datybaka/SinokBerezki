namespace SinokBerezki.Core.GameModels;

public class PlayerProfile
{
    public ulong UserId { get; set; }
    public List<CreatureModel> Monsters { get; set; } = new();

    // Дата последней успешной гачи (в формате YYYY-MM-DD по МСК)
    public string LastGachaDateMsk { get; set; } = string.Empty;
}