

namespace SinokBerezki.Core.Interfaces;

public interface IDiscordBot
{
    Task StartAsync(CancellationToken cancellationToken);
    Task StopAsync(CancellationToken cancellationToken);
}