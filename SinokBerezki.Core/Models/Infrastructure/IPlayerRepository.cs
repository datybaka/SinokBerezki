using SinokBerezki.Core.Models;

namespace SinokBerezki.Core.Interfaces;

public interface IPlayerRepository
{
    Task<Player?> GetByIdAsync(ulong discordId);
    Task SaveAsync(Player player);
    Task<bool> ExistsAsync(ulong discordId);
}