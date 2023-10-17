using EvoSC.Tool.Utils;
using EvoSC.Tool.Interfaces.Commands.NewCommands;
using Microsoft.Build.Construction;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace EvoSC.Tool.Commands.AddCommands;

public class AddModuleAddCommand : IAddCommand
{
    private readonly SolutionFile _solution;
    private readonly IAnsiConsole _console;
    public AddModuleAddCommand(SolutionFile solution, IAnsiConsole console)
    
    {
        _solution = solution;
        _console = console;
    }
    
    public async Task<int> ExecuteAsync(AddCommandOptions options)
    {
        var moduleType = await _console.ShowStringSelectionPromptAsync("Select Module Type:", "Internal", "External");
        
        if (moduleType == "Cancel")
        {
            return 0;
        }

        var moduleName = await AskNameAsync();
        var moduleTitle = await AskTitleAsync();
        var moduleDesc = await AskDescAsync();
        var moduleAuthor = await AskAuthorAsync();

        while (!await ConfirmInfoAsync(moduleName, moduleTitle, moduleDesc, moduleAuthor))
        {
            var toChange = await _console.ShowStringSelectionPromptAsync("What do you wish to edit?", "Name", "Title",
                "Description", "Author");

            switch (toChange)
            {
                case "Name":
                    moduleName = await AskNameAsync(moduleName);
                    break;
                case "Title":
                    moduleTitle = await AskTitleAsync(moduleTitle);
                    break;
                case "Description":
                    moduleDesc = await AskDescAsync(moduleDesc);
                    break;
                case "Author":
                    moduleAuthor = await AskAuthorAsync(moduleAuthor);
                    break;
            }
        }

        var status = new Status(_console)
        {
            SpinnerStyle = new Style(Color.HotPink)
        };

        await status.StartAsync("Creating project ...", async context =>
        {
            await Task.Delay(1000);
        });
        
        return 0;
    }

    private Task<string> AskNameAsync(string? defaultValue=null) => _console.ShowInputPromptAsync<string>("Unique Name:", defaultValue);
    private Task<string> AskTitleAsync(string? defaultValue=null) => _console.ShowInputPromptAsync<string>("Title:", defaultValue);
    private Task<string> AskDescAsync(string? defaultValue=null) => _console.ShowInputPromptAsync<string>("Description:", defaultValue);
    private Task<string> AskAuthorAsync(string? defaultValue=null) => _console.ShowInputPromptAsync<string>("Author:", defaultValue);

    private Task<bool> ConfirmInfoAsync(string name, string title, string desc, string author)
    {
        var table = new Table
        {
            Border = TableBorder.None,
            ShowHeaders = false
        };

        table.AddColumns("", "");
        table.AddRow("[bold]Name:[/]", $"[hotpink]{name}[/]");
        table.AddRow("[bold]Title:[/]", $"[hotpink]{title}[/]");
        table.AddRow("[bold]Description:[/]", $"[hotpink]{desc}[/]");
        table.AddRow("[bold]Author:[/]", $"[hotpink]{author}[/]");

        var panel = new Panel(table)
        {
            Border = new SquareBoxBorder(),
            Header = new PanelHeader("[bold]Module Info[/]"),
        };
        
        _console.WriteLine();
        _console.Write(panel);
        return _console.ConfirmAsync("Is this correct?");
    }
}
