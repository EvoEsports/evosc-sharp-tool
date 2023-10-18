using EvoSC.Tool.Interfaces;

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
}