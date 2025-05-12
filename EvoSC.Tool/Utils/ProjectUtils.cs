using EvoSC.Tool.Interfaces;
using Microsoft.Build.Construction;

namespace EvoSC.Tool.Utils;

public static class ProjectUtils
{
    /// <summary>
    /// Get the relative path to a reference path from a project directory within a solution.
    /// </summary>
    /// <param name="solution"></param>
    /// <param name="projectRoot">The root directory of the project.</param>
    /// <param name="referencePath">The path to get the relative path to from the root project directory.</param>
    /// <returns></returns>
    public static string GetRelativePath(this IEvoScSolution solution, string projectRoot,  string referencePath)
    {
        var solutionDir = Path.GetDirectoryName(solution.SolutionFilePath);
        return Path.GetRelativePath(projectRoot, Path.Combine(solutionDir, referencePath));
    }

    public static ProjectInSolution? GetCurrentProject(this IEvoScSolution solution) =>
        GetCurrentProject(solution, Environment.CurrentDirectory);

    private static ProjectInSolution? GetCurrentProject(IEvoScSolution solution, string dir)
    {
        if (string.IsNullOrEmpty(dir) || dir.Equals("/") || dir.Equals(solution.SolutionFilePath, StringComparison.Ordinal))
        {
            return null;
        }
        
        var fullDirPath = Path.GetFullPath(dir);
        fullDirPath = fullDirPath[^1] == '/' ? fullDirPath[..^1] : fullDirPath;

        var files = Directory.GetFiles(fullDirPath, "*.csproj", SearchOption.TopDirectoryOnly);

        if (files.Length == 0)
        {
            return GetCurrentProject(solution, Path.GetDirectoryName(fullDirPath));
        }

        var projectName = Path.GetFileNameWithoutExtension(files.First());
        var project = solution
            .SolutionFile
            .ProjectsInOrder
            .FirstOrDefault(p => p.ProjectName.Equals(projectName, StringComparison.Ordinal));

        return project;
    }

    public static ProjectInSolution? GetProject(this IEvoScSolution solution, string? projectName)
    {
        if (projectName == null)
        {
            return null;
        }
        
        return solution
            .SolutionFile
            .ProjectsInOrder
            .FirstOrDefault(p => p.ProjectName.Equals(projectName, StringComparison.Ordinal));
    }
}