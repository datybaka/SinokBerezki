using System.Text.Json;
using SinokBerezki.Core.Interfaces;
using SinokBerezki.Core.Models;

namespace SinokBerezki.Infrastructure.Repositories;

public class JsonPlayerRepository : IPlayerRepository
{
    private readonly string _directoryPath = Path.Combine(AppContext.BaseDirectory, "Data", "Players");

    public JsonPlayerRepository()
    {
        // Создаем директорию, если она еще не существует
        Directory.CreateDirectory(_directoryPath);
    }

    private string GetFilePath(ulong discordId) => Path.Combine(_directoryPath, $"{discordId}.json");

    public async Task<Player?> GetByIdAsync(ulong discordId)
    {
        var filePath = GetFilePath(discordId);
        if (!File.Exists(filePath)) return null;

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<Player>(json);
    }

    public async Task SaveAsync(Player player)
    {
        var filePath = GetFilePath(player.DiscordId);
        var options = new JsonSerializerOptions { WriteIndented = true };

        var json = JsonSerializer.Serialize(player, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public Task<bool> ExistsAsync(ulong discordId)
    {
        return Task.FromResult(File.Exists(GetFilePath(discordId)));
    }
}