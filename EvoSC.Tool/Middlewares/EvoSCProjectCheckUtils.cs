using System.CommandLine.Builder;
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
            
            context.BindingContext.AddService(typeof(SolutionFile), _ => solution);
            
            return next(context);
        });

    private static SolutionFile? FindSolutionFile(string currentPath)
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
                    return solution;
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