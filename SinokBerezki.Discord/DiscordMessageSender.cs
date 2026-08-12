using Discord;
using Discord.WebSocket;
using SinokBerezki.Application.Abstractions;
using SinokBerezki.Application.Models;
using SinokBerezki.Core.GameModels;
using SinokBerezki.Discordy.UI;
using SinokBerezki.Infrastructure.Repositories;

namespace SinokBerezki.Discordy.Services;

public class DiscordMessageSender : IMessageSender
{
    private readonly DiscordSocketClient _client;
    private readonly IStaticDataCatalog _catalog;

    public DiscordMessageSender(DiscordSocketClient client, IStaticDataCatalog catalog)
    {
        _client = client;
        _catalog = catalog;
    }

    public async Task SendIntroGuideAsync(ulong channelId, ulong authorId)
    {
        if (await _client.GetChannelAsync(channelId) is ISocketMessageChannel channel)
        {
            var (embed, components) = MonsterUiFactory.BuildIntroGuide(authorId);
            await channel.SendMessageAsync(embed: embed, components: components);
        }
    }

    public async Task SendMonsterMenuAsync(ulong channelId, CreatureModel monster, ulong authorId)
    {
        if (await _client.GetChannelAsync(channelId) is ISocketMessageChannel channel)
        {
            var (embed, components) = MonsterUiFactory.BuildMonsterMenu(monster, authorId, _catalog);
            await channel.SendMessageAsync(embed: embed, components: components);
        }
    }

    public async Task SendTextMessageAsync(ulong channelId, string text)
    {
        if (await _client.GetChannelAsync(channelId) is ISocketMessageChannel channel)
        {
            await channel.SendMessageAsync(text);
        }
    }

    public async Task SendHelpAsync(ulong channelId, IReadOnlyList<CommandInfo> commands)
    {
        if (await _client.GetChannelAsync(channelId) is ISocketMessageChannel channel)
        {
            var embed = MonsterUiFactory.BuildHelpEmbed(commands);
            await channel.SendMessageAsync(embed: embed);
        }
    }

    public async Task SendFarmMenuAsync(ulong channelId, IReadOnlyList<CreatureModel> monsters, int currentIndex, ulong authorId)
    {
        var channel = await _client.GetChannelAsync(channelId) as IMessageChannel;
        if (channel == null) return;

        var (embed, components) = MonsterUiFactory.BuildFarmMenu(monsters, currentIndex, authorId);

        await channel.SendMessageAsync(embed: embed, components: components);
    }
}