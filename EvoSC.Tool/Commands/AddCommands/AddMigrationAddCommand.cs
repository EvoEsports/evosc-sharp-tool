using EvoSC.Tool.Interfaces;
using EvoSC.Tool.Interfaces.Commands.NewCommands;
using EvoSC.Tool.Utils;
using EvoSC.Tool.Utils.Templates;
using Microsoft.Build.Construction;
using Spectre.Console;

namespace EvoSC.Tool.Commands.AddCommands;

public class AddMigrationAddCommand(IEvoScSolution solution, IAnsiConsole console) : IAddCommand
{
    public async Task<int> ExecuteAsync(AddCommandOptions options)
    {
        var solutionProject = FindProject(Path.GetFullPath(Environment.CurrentDirectory));
        solutionProject = await console.SelectProjectAsync(solution);

        if (solutionProject == null)
        {
            return 0;
        }

        var projectInfo = ProjectRootElement.Open(solutionProject.AbsolutePath);

        var migrationName = await console.ShowInputPromptAsync<string>("Migration Name: ", null);
        
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

        console.MarkupInterpolated($"[green]Migration successfully created in {filePath}[/]");
        
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
            var solutionProject = solution
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
}