//using System.Collections.Concurrent;
//using System.Text.RegularExpressions;
//using SinokBerezki.Application.Abstractions;
//using SinokBerezki.Core.Attributes;
//using SinokBerezki.Core.Interfaces;

//namespace SinokBerezki.Application.Commands;

//[Command("д", "Бросок кубиков: ?д (d20), ?дX (1dX), ?NдX+M или ?NдX-M")]
//public class RollCommandHandler : ICommandHandler
//{
//    private readonly IMessageSender _messageSender;
//    private static readonly ConcurrentDictionary<ulong, DateTime> _lastUsage = new();

//    public RollCommandHandler(IMessageSender messageSender)
//    {
//        _messageSender = messageSender;
//    }

//    public async Task HandleAsync(IMessage message)
//    {
//        var content = message.Content.Trim();
//        if (!content.StartsWith("?"))
//            return;

//        var commandPart = content[1..].TrimStart();

//        var match = Regex.Match(commandPart,
//            @"^(?:(\d+)\s*)?[дd](?:\s*(\d+))?(?:\s*([+-])\s*(\d+))?$",
//            RegexOptions.IgnoreCase);

//        if (!match.Success)
//            return;

//        int diceCount = match.Groups[1].Success ? int.Parse(match.Groups[1].Value) : 1;
//        int sideCount = match.Groups[2].Success ? int.Parse(match.Groups[2].Value) : 20;
//        int modifier = 0;

//        if (match.Groups[3].Success && match.Groups[4].Success)
//        {
//            int modVal = int.Parse(match.Groups[4].Value);
//            modifier = match.Groups[3].Value == "-" ? -modVal : modVal;
//        }

//        if (diceCount < 1 || diceCount > 1000)
//        {
//            await _messageSender.SendMessageAsync(message.ChannelId,
//                "❌ Количество кубиков должно быть от 1 до 1000.");
//            return;
//        }

//        if (sideCount < 2 || sideCount > 1000)
//        {
//            await _messageSender.SendMessageAsync(message.ChannelId,
//                "❌ Количество граней должно быть от 2 до 1000.");
//            return;
//        }

//        var rolls = new int[diceCount];
//        for (int i = 0; i < diceCount; i++)
//            rolls[i] = Random.Shared.Next(1, sideCount + 1);

//        int total = rolls.Sum() + modifier;

//        string formula = $"{diceCount}d{sideCount}";
//        if (modifier != 0)
//            formula += $" {(modifier > 0 ? "+" : "-")} {Math.Abs(modifier)}";

//        _lastUsage[message.AuthorId] = DateTime.UtcNow;

//        // Передаем чистые данные на уровень отображения
//        await _messageSender.SendRollResultAsync(message.ChannelId, formula, rolls, modifier, total, message.AuthorName);
//    }
//}