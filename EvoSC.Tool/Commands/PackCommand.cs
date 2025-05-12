using System.CommandLine;
using EvoSC.Tool.Interfaces;
using EvoSC.Tool.Interfaces.Utils;
using EvoSC.Tool.Utils;
using ILRepacking;
using Microsoft.Build.Evaluation;
using NuGet.ProjectModel;
using Spectre.Console;

namespace EvoSC.Tool.Commands;

public class PackCommand() : ToolCommand<PackCommandOptions, PackCommandHandler>
(
    name: "pack",
    description: "Links an external module and it's dependencies into one binary, making it ready to be used."
);

public class PackCommandOptions : IToolCommandOptions
{
    public string? Project { get; set; }
    public string? Configuration { get; set; }
    
    public string? Output { get; set; }

    public void AddOptions(Command command)
    {
        command.AddOption(new Option<string>(
            aliases: ["-p", "--project"],
            description: "Name of the project to pack."
        ));
        
        command.AddOption(new Option<string>(
            aliases: ["-c", "--configuration"],
            description: "The build configuration (eg. Debug or Release)"
        ));

        command.AddOption(new Option<string>(
            aliases: ["-o", "--output"],
            description: "The output file path of the packed binary."
        ));
    }
}

public class PackCommandHandler(IEvoScSolution solution, IAnsiConsole console) : ToolCommandHandler<PackCommandOptions>
{
    public override async Task<int> ExecuteAsync(PackCommandOptions options)
    {
        var project = solution.GetProject(options.Project)
                      ?? solution.GetCurrentProject()
                      ?? await console.SelectProjectAsync(solution);

        if (project == null)
        {
            console.MarkupLineInterpolated($"[red]Project not found.[/]");
            return -1;
        }
        
        var buildMode = options.Configuration ?? await console.ShowStringSelectionPromptAsync("Select Build Mode:", "Debug", "Release");
        var buildDirectory = Path.Combine(Path.GetDirectoryName(project.AbsolutePath) ?? string.Empty, "bin",
            buildMode, "net8.0");
        
        return await console.Status().StartAsync("Start packing assemblies...", async ctx =>
        {
            var assemblyFiles = new List<string>([
                Path.Combine(buildDirectory, $"{project.ProjectName}.dll")
            ]);

            ctx.Status("Reading nuget lock file ...");
            
            var lockFilePath = Path.Combine(Path.GetDirectoryName(project.AbsolutePath) ?? string.Empty, "obj",
                "project.assets.json");

            if (!File.Exists(lockFilePath))
            {
                console.MarkupLineInterpolated(
                    $"[red]Nuget lockfile not found in the project, please run [bold]dotnet restore[/].[/]");
                return -1;
            }

            var lockFile = LockFileUtilities.GetLockFile(lockFilePath, null);

            var projectDetails = new Project(project.AbsolutePath);

            ctx.Status("Getting project dependencies...");
            var projectDependencies = await GetProjectDependencyAssemblyFilesAsync(projectDetails, lockFile, -1);
            
            ctx.Status("Getting EvoSC# dependencies...");
            var evoscDependencies = await GetEvoScDependencies(lockFile);

            ctx.Status("Figuring dependencies needed...");
            var neededDependencies = projectDependencies.Except(evoscDependencies);
            foreach (var dependency in neededDependencies)
            {
                foreach (var runtimeTarget in dependency.RuntimeAssemblies)
                {
                    assemblyFiles.Add(Path.Combine(
                        lockFile.PackageFolders.First().Path,
                        dependency.Name.ToLowerInvariant(),
                        dependency.Version.ToNormalizedString(),
                        runtimeTarget.Path.Replace('/', Path.DirectorySeparatorChar)
                    ));
                }
            }

            ctx.Status("Packing assemblies and generating output binary...");
            var outputFile = options.Output ?? Path.Combine(buildDirectory, $"{project.ProjectName}.Packed.dll");
            var packerOptions = new RepackOptions
            {
                InputAssemblies = assemblyFiles.ToArray(),
                OutputFile = outputFile,
                UnionMerge = true,
                Log = true,
            };

            var packer = new ILRepack(packerOptions, console.CreateILRepackLogger());
            packer.Repack();

            console.MarkupLineInterpolated($"[green]Success! Output at:\n [bold]{outputFile}[/][/]");
            return 0;
        });
    }
    
    private async Task<HashSet<LockFileTargetLibrary>> GetProjectDependencyAssemblyFilesAsync(Project project, LockFile lockFile, int maxDepth)
    {
        var packageReferences = project.Items
            .Where(i => i.ItemType == "PackageReference")
            .Select(i => i.EvaluatedInclude)
            .ToHashSet();

        var topReferenceLibraries = lockFile.Targets
            .First().Libraries
            .Where(l => packageReferences.Contains(l.Name));

        var libraries = topReferenceLibraries.ToHashSet();

        foreach (var library in topReferenceLibraries)
        {
            libraries.UnionWith(GetDependencies(library, lockFile.Targets.First().Libraries, maxDepth));
        }
        
        return libraries;
    }

    private HashSet<LockFileTargetLibrary> GetDependencies(LockFileTargetLibrary library, IEnumerable<LockFileTargetLibrary> libraries, int maxDepth, int depth = 0)
    {
        if (maxDepth >= 0 && depth > maxDepth)
        {
            return new HashSet<LockFileTargetLibrary>();
        }
        
        var topDependencies = libraries
            .Where(l => library.Dependencies.Any(d => d.Id == l.Name))
            .ToHashSet();

        var dependencies = topDependencies.ToHashSet();
        foreach (var dependency in topDependencies)
        {
            var dependencyDependencies = GetDependencies(dependency, libraries, maxDepth, depth + 1);
            dependencies.UnionWith(dependencyDependencies);
        }
        
        return dependencies;
    }

    private async Task<HashSet<LockFileTargetLibrary>> GetEvoScDependencies(LockFile lockFile)
    {
        var evoScProjects = solution.SolutionFile.ProjectsInOrder
            .Where(p =>
                p.ProjectName.StartsWith("EvoSC", StringComparison.Ordinal)
                && !p.ProjectName.StartsWith("EvoSC.Testing", StringComparison.Ordinal)
                && !p.ProjectName.Contains("SourceGeneration", StringComparison.Ordinal)
            );

        var libraries = new HashSet<LockFileTargetLibrary>();

        foreach (var project in evoScProjects)
        {
            var projectDetails = new Project(project.AbsolutePath);
            var projectDependencies = await GetProjectDependencyAssemblyFilesAsync(projectDetails, lockFile, 0);
            libraries.UnionWith(projectDependencies);
        }
        
        return libraries;
    }
}
