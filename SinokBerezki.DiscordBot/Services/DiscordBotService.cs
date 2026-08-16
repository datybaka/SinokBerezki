using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SinokBerezki.Application.Abstractions;
using SinokBerezki.Core.Interfaces;
using SinokBerezki.Core.Models;

namespace SinokBerezki.DiscordBot.Services;

public class DiscordBotService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiscordBotService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public DiscordBotService(
        DiscordSocketClient client,
        IConfiguration configuration,
        ILogger<DiscordBotService> logger,
        IServiceProvider serviceProvider)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _serviceProvider = serviceProvider;

        // Подписываемся на события 
        _client.Log += LogAsync;
        _client.Ready += ReadyAsync;
        _client.MessageReceived += HandleMessageAsync; // Обработчик сообщений
        _client.InteractionCreated += HandleInteractionAsync;
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
        // Отписываемся от событий при остановке (хорошая практика)
        _client.MessageReceived -= HandleMessageAsync;
        _client.InteractionCreated -= HandleInteractionAsync;

        await _client.StopAsync();
        await _client.LogoutAsync();
    }

    private async Task HandleMessageAsync(SocketMessage messageParam)
    {
        if (messageParam is not SocketUserMessage message) return;
        if (message.Author.IsBot) return;
        if (!message.Content.StartsWith('?')) return;

        var commandName = message.Content.Substring(1).Split(' ').FirstOrDefault()?.ToLower();
        if (string.IsNullOrEmpty(commandName)) return;

        using var scope = _serviceProvider.CreateScope();
        var commands = scope.ServiceProvider.GetServices<IBotCommand>();
        var command = commands.FirstOrDefault(c => c.Name == commandName);

        if (command != null)
        {
            var response = await command.ExecuteAsync();

            // Теперь передаем и Embed, и Component
            await message.Channel.SendMessageAsync(
                embed: response.Embed,
                components: response.Component);
        }
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        if (interaction is SocketMessageComponent component)
        {
            if (component.Data.CustomId == "enter_game_btn")
            {
                var userId = component.User.Id;
                var username = component.User.Username;

                // Создаем scope для доступа к scoped/singleton зависимостям инфраструктуры
                using var scope = _serviceProvider.CreateScope();
                var playerRepository = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();

                var player = await playerRepository.GetByIdAsync(userId);

                if (player == null)
                {
                    // Создаем нового игрока, если файла еще нет
                    player = new Player
                    {
                        DiscordId = userId,
                        Username = username
                    };

                    await playerRepository.SaveAsync(player);

                    await component.RespondAsync(
                        $"🎮 Добро пожаловать в игру, **{username}**.",
                        ephemeral: true);
                }
                else
                {
                    await component.RespondAsync(
                        $"С возвращением, **{username}**.",
                        ephemeral: true);
                }
            }
        }
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