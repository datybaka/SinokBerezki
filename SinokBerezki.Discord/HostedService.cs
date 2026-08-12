using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SinokBerezki.Core.Interfaces;
using SinokBerezki.Discordy.Handlers;

namespace SinokBerezki.Discordy;

public class HostedService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly ILogger<HostedService> _logger;
    private readonly IEnumerable<ICommandHandler> _commandHandlers;
    private readonly MonsterButtonHandler _buttonHandler;
    private readonly string _token;

    public HostedService(
        DiscordSocketClient client,
        string token,
        ILogger<HostedService> logger,
        IEnumerable<ICommandHandler> commandHandlers,
        MonsterButtonHandler buttonHandler)
    {
        _client = client;
        _token = token;
        _logger = logger;
        _commandHandlers = commandHandlers;
        _buttonHandler = buttonHandler;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _client.MessageReceived += OnMessageReceivedAsync;
        _client.ButtonExecuted += _buttonHandler.HandleButtonAsync;
        _client.Log += OnLogAsync;

        await _client.LoginAsync(TokenType.Bot, _token);
        await _client.StartAsync();
        _logger.LogInformation("Discord bot started");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _client.ButtonExecuted -= _buttonHandler.HandleButtonAsync;
        _client.MessageReceived -= OnMessageReceivedAsync;
        await _client.StopAsync();
        await _client.LogoutAsync();
        _logger.LogInformation("Discord bot stopped");
    }

    private Task OnLogAsync(LogMessage log)
    {
        _logger.Log(log.Severity.ToLogLevel(), log.Exception, log.Message);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceivedAsync(SocketMessage socketMessage)
    {
        if (socketMessage.Author.IsBot) return;

        var message = new DiscordMessage(socketMessage);
        foreach (var handler in _commandHandlers)
        {
            await handler.HandleAsync(message);
        }
    }

    private class DiscordMessage : Core.Interfaces.IMessage
    {
        private readonly SocketMessage _socketMessage;

        public DiscordMessage(SocketMessage socketMessage)
        {
            _socketMessage = socketMessage;
        }

        public string AuthorName => _socketMessage.Author.GlobalName ?? _socketMessage.Author.Username;
        public string Content => _socketMessage.Content;
        public ulong AuthorId => _socketMessage.Author.Id;
        public ulong ChannelId => _socketMessage.Channel.Id;
    }
}