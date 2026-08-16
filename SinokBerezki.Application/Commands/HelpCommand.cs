using Discord;
using SinokBerezki.Application.Abstractions;

namespace SinokBerezki.Application.Commands;

public class HelpCommand : IBotCommand
{
    public string Name => "помощь";

    public Task<CommandResponse> ExecuteAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("📚 Список команд")
            .WithDescription("Вот что я умею на данный момент:")
            .AddField("`?помощь`", "Выводит список всех доступных команд.", inline: false)
            .AddField("`?монстры`", "Команда в разработке (заглушка).", inline: false)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp()
            .Build();

        return Task.FromResult(new CommandResponse { Embed = embed });
    }
}