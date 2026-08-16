namespace SinokBerezki.Core.Models;

public class Player
{
    public ulong DiscordId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}