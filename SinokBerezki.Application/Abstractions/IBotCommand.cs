using Discord;
using SinokBerezki.Application.Commands;

namespace SinokBerezki.Application.Abstractions;

public interface IBotCommand
{
    // Имя команды, по которому она будет вызываться (без префикса)
    string Name { get; }

    // Метод выполнения, возвращающий готовый визуал
    Task<CommandResponse> ExecuteAsync();
}
