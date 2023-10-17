using EvoSC.Tool.Commands;

namespace EvoSC.Tool.Interfaces.Commands.NewCommands;

public interface IAddCommand
{
    public Task<int> ExecuteAsync(AddCommandOptions options);
}
