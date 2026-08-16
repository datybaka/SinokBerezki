using Discord;

namespace SinokBerezki.Application.Commands;

public class CommandResponse
{
    public required Embed Embed { get; init; }
    public MessageComponent? Component { get; init; } // UI-компоненты опциональны
}