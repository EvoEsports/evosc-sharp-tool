using System.CommandLine;

namespace EvoSC.Tool.Commands;

public class CommandOptionAttribute<T> : Attribute
{
    public Option<T> Option { get; set; }

    public CommandOptionAttribute(string name, string? description = null) =>
        Option = new Option<T>(name, description);

    public CommandOptionAttribute(string name, T defaultValue, string? description = null) =>
        Option = new Option<T>(name, () => defaultValue, description);
    
    public CommandOptionAttribute(string[] aliases, string? description = null) =>
        Option = new Option<T>(aliases, description);
    
    public CommandOptionAttribute(string[] aliases, T defaultValue, string? description = null) =>
        Option = new Option<T>(aliases, () => defaultValue, description);
}