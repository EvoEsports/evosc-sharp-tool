using System.CommandLine;

namespace EvoSC.Tool.Interfaces.Utils;

public interface IToolCommandOptions
{
    public void AddOptions(Command command);
}