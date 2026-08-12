namespace SinokBerezki.Application.Models;

public class CommandInfo
{
    public CommandInfo(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}