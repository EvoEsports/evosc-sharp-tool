using EvoSC.Tool.Interfaces.Utils;
using Spectre.Console;

namespace EvoSC.Tool.Utils;

public abstract class ToolCommandHandler<TOptions> : IToolCommandHandler<TOptions> 
    where TOptions : class, IToolCommandOptions
{
    protected IAnsiConsole Console { get; private set; }
    
    public abstract Task<int> ExecuteAsync(TOptions options);

    void IToolCommandHandler<TOptions>.SetConsole(IAnsiConsole console)
    {
        Console = console;
    }
}
