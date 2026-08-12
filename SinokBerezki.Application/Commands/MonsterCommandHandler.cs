using SinokBerezki.Application.Abstractions;
using SinokBerezki.Core.Attributes;
using SinokBerezki.Core.Interfaces;
using SinokBerezki.Infrastructure.Repositories;

namespace SinokBerezki.Application.Commands;

[Command("монстры", "Главное меню: гайд, управление отрядом и бои")]
public class MonsterCommandHandler : ICommandHandler
{
    private readonly IMessageSender _messageSender;
    private readonly IPlayerRepository _repository;

    public MonsterCommandHandler(IMessageSender messageSender, IPlayerRepository repository)
    {
        _messageSender = messageSender;
        _repository = repository;
    }

    public async Task HandleAsync(IMessage message)
    {
        var content = message.Content.Trim();
        if (!content.Equals("?монстры", StringComparison.OrdinalIgnoreCase)) return;

        var profile = await _repository.GetOrCreateProfileAsync(message.AuthorId);

        if (profile.Monsters.Count == 0)
        {
            await _messageSender.SendIntroGuideAsync(message.ChannelId, message.AuthorId);
        }
        else
        {
            // Применяем пассивную регенерацию ко всем монстрам в загоне перед показом
            foreach (var monster in profile.Monsters)
            {
                monster.ApplyOfflineRegen();
            }
            // Опционально: если ApplyOfflineRegen изменяет данные, стоит их сохранить
            // await _repository.SaveProfileAsync(profile);

            // Выводим меню Фермы, начиная с 0-го индекса
            await _messageSender.SendFarmMenuAsync(message.ChannelId, profile.Monsters, 0, message.AuthorId);
        }
    }
}