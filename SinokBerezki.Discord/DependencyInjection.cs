using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SinokBerezki.Application.Abstractions;
using SinokBerezki.Core.Interfaces;
using SinokBerezki.Discordy.Handlers;
using SinokBerezki.Discordy.Services;

namespace SinokBerezki.Discordy;

public static class DiscordyServicesExtensions
{
    public static IServiceCollection AddDiscordyServices(this IServiceCollection services, string token)
    {
        // Регистрируем клиент как синглтон
        services.AddSingleton<DiscordSocketClient>(sp =>
        {
            var config = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds          // для доступа к каналам
                               | GatewayIntents.GuildMessages  // для сообщений в гильдиях
                               | GatewayIntents.MessageContent // для чтения содержимого
            };
            return new DiscordSocketClient(config);
        });

        services.AddSingleton(token);

        // Реализация абстракций из Application
        services.AddSingleton<IMessageSender, DiscordMessageSender>();

        // Обработчик кнопок и сам HostedService
        services.AddSingleton<MonsterButtonHandler>();
        services.AddHostedService<HostedService>();

        return services;
    }
}