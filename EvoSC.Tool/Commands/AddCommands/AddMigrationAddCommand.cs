using EvoSC.Tool.Interfaces;
using EvoSC.Tool.Interfaces.Commands.NewCommands;
using EvoSC.Tool.Utils;
using EvoSC.Tool.Utils.Templates;
using Microsoft.Build.Construction;
using Spectre.Console;

namespace EvoSC.Tool.Commands.AddCommands;

public class AddMigrationAddCommand : IAddCommand
{
    private readonly IEvoScSolution _solution;
    private readonly IAnsiConsole _console;
    
    public AddMigrationAddCommand(IEvoScSolution solution, IAnsiConsole console)
    {
        _solution = solution;
        _console = console;
    }
    
    public async Task<int> ExecuteAsync(AddCommandOptions options)
    {
        var solutionProject = FindProject(Path.GetFullPath(Environment.CurrentDirectory));
        solutionProject = await SelectProjectAsync();

        if (solutionProject == null)
        {
            return 0;
        }

        var projectInfo = ProjectRootElement.Open(solutionProject.AbsolutePath);

        var migrationName = await _console.ShowInputPromptAsync<string>("Migration Name: ", null);
        
        if (projectInfo == null)
        {
            throw new InvalidOperationException($"Failed to open the project: {solutionProject.AbsolutePath}");
        }
        
        var migrationsDir = Path.Combine(Path.GetDirectoryName(solutionProject.AbsolutePath), "Migrations");

        if (!Directory.Exists(migrationsDir))
        {
            Directory.CreateDirectory(migrationsDir);
        }
        
        var dt = DateTime.Now;
        var migrationTemplate = new MigrationFileTemplate
        {
            Namespace = projectInfo.Properties.First(p => p.Name.Equals("RootNamespace", StringComparison.Ordinal)).Value,
            UnixTimeStamp = new DateTimeOffset(dt).ToUnixTimeSeconds(),
            MigrationName = migrationName
        };

        var fileName = $"{dt.ToString("yyyyMMddhhmm")}_{migrationName}.cs";
        var filePath = Path.Combine(migrationsDir, fileName);
        await File.WriteAllTextAsync(filePath, migrationTemplate.TransformText());

        _console.MarkupInterpolated($"[green]Migration successfully created in {filePath}[/]");
        
        return 0;
    }

    private ProjectInSolution? FindProject(string dir)
    {
        foreach (var file in Directory.GetFiles(dir))
        {
            if (!Path.GetExtension(file).Equals(".csproj", StringComparison.Ordinal))
            {
                continue;
            }

            var project = ProjectRootElement.Open(file);

            if (project == null)
            {
                continue;
            }

            var projectName = Path.GetFileNameWithoutExtension(file);
            var solutionProject = _solution
                .SolutionFile
                .ProjectsInOrder
                .FirstOrDefault(p => p.ProjectName.Equals(projectName, StringComparison.Ordinal));

            if (solutionProject != null)
            {
                return solutionProject;
            }
        }

        var parentDir = Path.GetDirectoryName(dir);

        if (string.IsNullOrEmpty(parentDir) || parentDir.Equals(dir, StringComparison.Ordinal))
        {
            return null;
        }

        return FindProject(parentDir);
    }

    private async Task<ProjectInSolution?> SelectProjectAsync()
    {
        var searchInput = await _console.ShowInputPromptAsync<string>("Project to add migration to:", null);
        var projects = _solution
            .SolutionFile
            .ProjectsInOrder
            .Select(p => new
            {
                Name = p.ProjectName,
                MatchIndex = p.ProjectName.ToLower().IndexOf(searchInput.ToLower(), StringComparison.Ordinal)
            })
            .Where(p => p.MatchIndex >= 0) 
            .OrderBy(p => p.MatchIndex)
            .ThenBy(p => p.Name)
            .Select(p => p.Name)
            .ToArray();
        
        var selection = await _console.ShowStringSelectionPromptAsync($"Projects matching {searchInput}:", projects);

        return selection.Equals("Cancel", StringComparison.Ordinal)
            ? null
            : _solution.SolutionFile.ProjectsInOrder.First(p =>
                p.ProjectName.Equals(selection, StringComparison.Ordinal));
    }
}