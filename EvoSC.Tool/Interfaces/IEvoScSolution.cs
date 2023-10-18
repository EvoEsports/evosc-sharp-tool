using Microsoft.Build.Construction;

namespace EvoSC.Tool.Interfaces;

public interface IEvoScSolution
{
    public SolutionFile SolutionFile { get; }
    public string SolutionFilePath { get; }

    public Task RefreshAsync();
}