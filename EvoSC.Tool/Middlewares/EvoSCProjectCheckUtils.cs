using System.CommandLine.Builder;
using EvoSC.Tool.Interfaces;
using EvoSC.Tool.Utils;
using Microsoft.Build.Construction;

namespace EvoSC.Tool.Middlewares;

public static class EvoSCProjectCheckUtils
{
    public static CommandLineBuilder AddEvoSCProjectCheck(this CommandLineBuilder builder) =>
        builder.AddMiddleware((context, next) =>
        {
            var solution = FindSolutionFile(Path.GetFullPath(Environment.CurrentDirectory));

            if (solution == null)
            {
                Console.Error.WriteLine("Not in the EvoSC solution project.");
                return Task.CompletedTask;
            }
            
            context.BindingContext.AddService<IEvoScSolution>(_ => solution);
            
            return next(context);
        });

    private static IEvoScSolution? FindSolutionFile(string currentPath)
    {
        foreach (var file in Directory.GetFiles(currentPath))
        {
            if (!Path.GetExtension(file).Equals(".sln"))
            {
                continue;
            }

            var solution = SolutionFile.Parse(file);

            foreach (var project in solution.ProjectsInOrder)
            {
                if (project.ProjectName.Equals("EvoSC", StringComparison.Ordinal))
                {
                    return new EvoScSolution
                    {
                        SolutionFile = solution,
                        SolutionFilePath = Path.GetFullPath(file)
                    };
                }
            }
        }

        var rootPath = Path.GetDirectoryName(currentPath);

        if (string.IsNullOrEmpty(rootPath) || rootPath.Equals(currentPath, StringComparison.Ordinal))
        {
            return null;
        }
        
        return FindSolutionFile(rootPath);
    }
}