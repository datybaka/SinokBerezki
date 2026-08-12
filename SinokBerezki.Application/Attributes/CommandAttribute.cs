// Core/Attributes/CommandAttribute.cs
namespace SinokBerezki.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CommandAttribute : Attribute
{
    public string Name { get; }
    public string Description { get; }

    public CommandAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}