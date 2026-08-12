
namespace SinokBerezki.Core.Interfaces;

public interface IMessage
{
    string Content { get; }
    ulong AuthorId { get; }
    ulong ChannelId { get; }
    string AuthorName { get; }
}
