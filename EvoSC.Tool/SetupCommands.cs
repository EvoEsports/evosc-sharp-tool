using System.CommandLine;
using EvoSC.Tool.Commands;

namespace EvoSC.Tool;

public static class SetupCommands
{
    public static RootCommand Setup()
    {
        var root = new RootCommand
        {
            new AddCommand()
        };

        root.Description = "EvoSC# tool.";

        return root;
    }
}