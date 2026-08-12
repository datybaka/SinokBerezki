using System.Reflection;
using SinokBerezki.Application.Abstractions;
using SinokBerezki.Application.Models;
using SinokBerezki.Core.Attributes;
using SinokBerezki.Core.Interfaces;

namespace SinokBerezki.Application.Services;

public class CommandMetadataProvider : ICommandMetadataProvider
{
    private readonly List<CommandInfo> _commands;

    public CommandMetadataProvider()
    {
        _commands = LoadCommandsFromAssembly();
    }

    public IReadOnlyList<CommandInfo> GetCommands() => _commands;

    private List<CommandInfo> LoadCommandsFromAssembly()
    {
        var commandList = new List<CommandInfo>();
        var assembly = Assembly.GetAssembly(typeof(CommandMetadataProvider));
        if (assembly == null) return commandList;

        // Ищем все классы, реализующие ICommandHandler и имеющие атрибут [Command]
        var commandHandlerTypes = assembly.GetTypes()
            .Where(t => typeof(ICommandHandler).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in commandHandlerTypes)
        {
            var attribute = type.GetCustomAttribute<CommandAttribute>();
            if (attribute != null)
            {
                commandList.Add(new CommandInfo(attribute.Name, attribute.Description));
            }
        }

        return commandList;
    }
}