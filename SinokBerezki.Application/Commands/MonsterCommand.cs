using Discord;
using SinokBerezki.Application.Abstractions;

namespace SinokBerezki.Application.Commands;

public class MonstersCommand : IBotCommand
{
    public string Name => "монстры";

    public Task<CommandResponse> ExecuteAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("⚔️ Добро пожаловать в мир Монстров!")
            .WithDescription("В этом мире обитают уникальные создания, чья сила кроется в **генах** и **магии**.\n\n" +
                             "Отправляйтесь в PvE-сражения против диких особей! Используйте мощь стихийной магии, " +
                             "чтобы получить тактическое преимущество на разнообразных стихийных полях битвы. " +
                             "Собирайте пачку монстров, адаптируйтесь и побеждайте!")
            .WithColor(Color.DarkGreen)
            .Build();

        // Создаем кнопку с уникальным идентификатором (customId)
        var component = new ComponentBuilder()
            .WithButton("Войти в игру", "enter_game_btn", ButtonStyle.Success, new Emoji("🎮"))
            .Build();

        return Task.FromResult(new CommandResponse
        {
            Embed = embed,
            Component = component
        });
    }
}