//using Discord;
//using SinokBerezki.Application.Abstractions;
//using SinokBerezki.Application.Models;

//namespace SinokBerezki.Application.Commands;

//public static class MonsterMenuBuilder
//{
//    public static async Task SendMonsterMenuAsync(IMessageSender sender, ulong channelId, Creature monster, ulong authorId)
//    {
//        // Применяем оффлайн-регенерацию перед показом
//        monster.ApplyOfflineRegen();

//        var embed = new EmbedData
//        {
//            Title = $"👾 {monster.Name}",
//            Color = "Green",
//            Description = "Главное меню управления существом.",
//        };

//        embed.Fields.Add(new EmbedFieldData { Name = "❤️ HP", Value = $"{monster.CurrentHp} / {monster.BaseMaxHp}", Inline = true });
//        embed.Fields.Add(new EmbedFieldData { Name = "💧 MP", Value = $"{monster.CurrentMp} / {monster.BaseMaxMp}", Inline = true });
//        embed.Fields.Add(new EmbedFieldData { Name = "⚡ Инициатива", Value = monster.BaseInitiative.ToString(), Inline = true });

//        var components = new ComponentBuilder()
//            .WithButton("Бой", $"btn_battle_{authorId}", ButtonStyle.Danger)
//            .WithButton("Приручить", $"btn_gacha_{authorId}", ButtonStyle.Primary)
//            .WithButton("Гены", $"btn_genes_{monster.Id}_{authorId}", ButtonStyle.Secondary)
//            .WithButton("Заклинания", $"btn_spells_{monster.Id}_{authorId}", ButtonStyle.Secondary)
//            .Build();

//        await sender.SendEmbedAsync(channelId, embed, components);
//    }
//}