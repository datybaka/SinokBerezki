using Discord.WebSocket;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SinokBerezki.Application;
using SinokBerezki.DiscordBot.Services;
using SinokBerezki.Infrastructure;

Env.TraversePath().Load();

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    // 1. Конфигурируем и регистрируем клиент Discord
    services.AddSingleton(new DiscordSocketConfig
    {
        GatewayIntents = Discord.GatewayIntents.AllUnprivileged | Discord.GatewayIntents.MessageContent
    });
    services.AddSingleton<DiscordSocketClient>();

    services.AddInfrastructure();
    services.AddApplication();
    services.AddHostedService<DiscordBotService>();
});

var host = builder.Build();

await host.RunAsync();