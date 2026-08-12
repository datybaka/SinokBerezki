using SinokBerezki.Core.GameModels;
using System.Collections.Concurrent;
using System.Text.Json;

namespace SinokBerezki.Infrastructure.Repositories;

public interface IPlayerRepository
{
    Task<PlayerProfile> GetOrCreateProfileAsync(ulong userId);
    Task SaveProfileAsync(PlayerProfile profile);
}

public class JsonPlayerRepository : IPlayerRepository
{
    private readonly string _storageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "players");
    private static readonly ConcurrentDictionary<ulong, SemaphoreSlim> _locks = new();

    public JsonPlayerRepository()
    {
        Directory.CreateDirectory(_storageFolder);
    }

    private SemaphoreSlim GetLock(ulong userId) => _locks.GetOrAdd(userId, _ => new SemaphoreSlim(1, 1));

    public async Task<PlayerProfile> GetOrCreateProfileAsync(ulong userId)
    {
        var myLock = GetLock(userId);
        await myLock.WaitAsync();
        try
        {
            string filePath = Path.Combine(_storageFolder, $"{userId}.json");
            if (!File.Exists(filePath))
            {
                var newProfile = new PlayerProfile { UserId = userId };
                return newProfile;
            }

            string json = await File.ReadAllTextAsync(filePath);
            var profile = JsonSerializer.Deserialize<PlayerProfile>(json) ?? new PlayerProfile { UserId = userId };

            // Актуализация оффлайн-регенерации у всех монстров при загрузке профиля
            foreach (var monster in profile.Monsters)
            {
                monster.ApplyOfflineRegen();
            }

            return profile;
        }
        finally
        {
            myLock.Release();
        }
    }

    public async Task SaveProfileAsync(PlayerProfile profile)
    {
        var myLock = GetLock(profile.UserId);
        await myLock.WaitAsync();
        try
        {
            string filePath = Path.Combine(_storageFolder, $"{profile.UserId}.json");
            string json = JsonSerializer.Serialize(profile, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json);
        }
        finally
        {
            myLock.Release();
        }
    }
}