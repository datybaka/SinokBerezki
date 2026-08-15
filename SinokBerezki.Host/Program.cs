using Discord.WebSocket;
using DotNetEnv;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SinokBerezki.DiscordBot.Services;

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

    // 2. Регистрируем сервис бота именно как HostedService
    services.AddHostedService<DiscordBotService>();
});

var host = builder.Build();

await host.RunAsync();