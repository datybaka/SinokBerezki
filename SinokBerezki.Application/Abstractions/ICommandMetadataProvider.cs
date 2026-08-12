using SinokBerezki.Application.Models;

namespace SinokBerezki.Application.Abstractions;

public interface ICommandMetadataProvider
{
    IReadOnlyList<CommandInfo> GetCommands();
}
