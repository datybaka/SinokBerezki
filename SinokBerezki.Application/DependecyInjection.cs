using Microsoft.Extensions.DependencyInjection;
using SinokBerezki.Application.Abstractions;
using SinokBerezki.Application.Commands;

namespace SinokBerezki.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Регистрируем все команды. 
        // Позже сюда можно прикрутить рефлексию (Scrutor), чтобы не вписывать каждую команду руками.
        services.AddScoped<IBotCommand, HelpCommand>();
        services.AddScoped<IBotCommand, MonstersCommand>();

        return services;
    }
}