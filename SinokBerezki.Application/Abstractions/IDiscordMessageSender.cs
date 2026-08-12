using SinokBerezki.Core.GameModels;

namespace SinokBerezki.Application.Abstractions;

public interface IMessageSender
{
    Task SendIntroGuideAsync(ulong channelId, ulong authorId);
    Task SendMonsterMenuAsync(ulong channelId, CreatureModel monster, ulong authorId);
    Task SendTextMessageAsync(ulong channelId, string text);
    Task SendFarmMenuAsync(ulong channelId, IReadOnlyList<CreatureModel> monsters, int currentIndex, ulong authorId);
}