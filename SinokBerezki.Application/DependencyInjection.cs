using Microsoft.Extensions.DependencyInjection;
using SinokBerezki.Application.Abstractions;
using SinokBerezki.Application.Services;
using SinokBerezki.Core.Interfaces;

namespace SinokBerezki.Application;

public static class ApplicationServicesExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<CreatureGenerator>();
        services.AddSingleton<GachaService>();

        // 2. Автоматическая регистрация всех команд (ICommandHandler) из текущей сборки
        var commandHandlerType = typeof(ICommandHandler);
        var handlers = typeof(ApplicationServicesExtensions).Assembly.GetTypes()
            .Where(t => commandHandlerType.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in handlers)
        {
            // Регистрируем класс напрямую и как интерфейс ICommandHandler
            services.AddSingleton(type);
            services.AddSingleton(commandHandlerType, sp => sp.GetRequiredService(type));
        }

        services.AddTransient<ICommandMetadataProvider, CommandMetadataProvider>();

        return services;
    }
}