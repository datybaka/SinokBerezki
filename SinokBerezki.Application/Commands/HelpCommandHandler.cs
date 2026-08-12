//using SinokBerezki.Application.Abstractions;
//using SinokBerezki.Core.Attributes;
//using SinokBerezki.Core.Interfaces;

//namespace SinokBerezki.Application.Commands;

//[Command("помощь", "Показать список всех доступных команд")]
//public class HelpCommandHandler : ICommandHandler
//{
//    private readonly IMessageSender _messageSender;
//    private readonly ICommandMetadataProvider _metadataProvider;

//    public HelpCommandHandler(IMessageSender messageSender, ICommandMetadataProvider metadataProvider)
//    {
//        _messageSender = messageSender;
//        _metadataProvider = metadataProvider;
//    }

//    public async Task HandleAsync(Core.Interfaces.IMessage message)
//    {
//        var content = message.Content.Trim();
//        if (!content.Equals("?помощь", StringComparison.OrdinalIgnoreCase))
//            return;

//        var commands = _metadataProvider.GetCommands();
//        await _messageSender.SendHelpAsync(message.ChannelId, commands);
//    }
//}