using System.CommandLine;
using EvoSC.Tool.Commands.AddCommands;
using EvoSC.Tool.Interfaces.Commands.NewCommands;
using EvoSC.Tool.Interfaces.Utils;
using EvoSC.Tool.Utils;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace EvoSC.Tool.Commands;

public class AddCommand : ToolCommand<AddCommandOptions, AddCommandHandler>
{
    public AddCommand() : base(
        name: "add",
        description: "Create a new item for EvoSC#.")
    {
    }
}

public class AddCommandOptions : IToolCommandOptions
{
    public void AddOptions(Command command)
    {
    }
}

public class AddCommandHandler : ToolCommandHandler<AddCommandOptions>
{
    private readonly IServiceProvider _services;
    
    public AddCommandHandler(IServiceProvider services)
    {
        _services = services;
    }
    
    public override async Task<int> ExecuteAsync(AddCommandOptions options)
    {
        var addSelection = await Console.ShowStringSelectionPromptAsync("What do you want to add?",
            "Module",
            "Migration"
        );

        return addSelection switch
        {
            "Module" => await ExecuteAddAsync(typeof(AddModuleAddCommand), options),
            "Migration" => await ExecuteAddAsync(typeof(AddMigrationAddCommand), options),
            _ => 0
        };
    }

    private Task<int> ExecuteAddAsync(Type t, AddCommandOptions options, params object[] args)
    {
        var instance = ActivatorUtilities.CreateInstance(_services, t, args) as IAddCommand;

        if (instance == null)
        {
            throw new InvalidOperationException($"Failed to instantiate add command class for type '{t}'.");
        }

        return instance.ExecuteAsync(options);
    }

    private Task<string> MainPromptAsync()
    {
        var mainPrompt = new SelectionPrompt<string>()
            .Title("What do you want to create?")
            .AddChoices("Module");
        mainPrompt.HighlightStyle = new Style(foreground: Color.HotPink);
        mainPrompt.AddChoice("Cancel");

        return mainPrompt.ShowAsync(Console, CancellationToken.None);
    }
}
