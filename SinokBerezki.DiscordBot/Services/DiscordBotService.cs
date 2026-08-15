using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SinokBerezki.DiscordBot.Services;

public class DiscordBotService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiscordBotService> _logger;

    public DiscordBotService(
        DiscordSocketClient client,
        IConfiguration configuration,
        ILogger<DiscordBotService> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;

        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var token = Environment.GetEnvironmentVariable("TOKEN") ?? _configuration["TOKEN"];

        if (string.IsNullOrEmpty(token))
        {
            _logger.LogError("Discord токен не найден в .env файле.");
            return;
        }

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private Task LogAsync(LogMessage logMessage)
    {
        _logger.LogInformation(logMessage.Message);
        return Task.CompletedTask;
    }

    private Task ReadyAsync()
    {
        _logger.LogInformation($"Бот успешно подключен как {_client.CurrentUser}");
        return Task.CompletedTask;
    }
}