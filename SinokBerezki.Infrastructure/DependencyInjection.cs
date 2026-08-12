using Microsoft.Extensions.DependencyInjection;
using SinokBerezki.Infrastructure.Repositories;


namespace SinokBerezki.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // Репозитории и хранилища
        services.AddSingleton<IPlayerRepository, JsonPlayerRepository>();

        // Загрузчик JSON-каталогов
        services.AddSingleton<IStaticDataCatalog>(sp =>
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string spellsPath = Path.Combine(basePath, "data", "spells.json");
            string abilitiesPath = Path.Combine(basePath, "data", "abilities.json");

            return new StaticDataCatalog(spellsPath, abilitiesPath);
        });

        return services;
    }
}