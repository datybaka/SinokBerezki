using DotNetEnv;
using Microsoft.Extensions.Hosting;
using SinokBerezki.Application;
using SinokBerezki.Discordy;
using SinokBerezki.Infrastructure;

Env.Load(); // загружаем .env из корня решения (или указываем путь)

var token = Environment.GetEnvironmentVariable("TOKEN")
           ?? throw new InvalidOperationException("TOKEN not set in .env");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Регистрируем сервисы каждого модуля
        services.AddApplicationServices();
        services.AddInfrastructureServices();
        services.AddDiscordyServices(token);

        // Можно добавить логгирование, конфигурацию и т.д.
    })      
    .Build();

await host.RunAsync();