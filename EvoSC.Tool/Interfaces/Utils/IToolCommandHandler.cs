using Spectre.Console;

namespace EvoSC.Tool.Interfaces.Utils;

public interface IToolCommandHandler<TOptions> where TOptions : class, IToolCommandOptions
{
    public Task<int> ExecuteAsync(TOptions options);
    public void SetConsole(IAnsiConsole console);
}
