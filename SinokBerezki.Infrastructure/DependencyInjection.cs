using Microsoft.Extensions.DependencyInjection;
using SinokBerezki.Core.Interfaces;
using SinokBerezki.Infrastructure.Repositories;

namespace SinokBerezki.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IPlayerRepository, JsonPlayerRepository>();
        return services;
    }
}