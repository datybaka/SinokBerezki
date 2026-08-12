// Core/Interfaces/ICommandHandler.cs
namespace SinokBerezki.Core.Interfaces;

public interface ICommandHandler
{
    Task HandleAsync(IMessage message);
}