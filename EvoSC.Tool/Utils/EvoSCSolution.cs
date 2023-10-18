using EvoSC.Tool.Interfaces;
using Microsoft.Build.Construction;

namespace EvoSC.Tool.Utils;

public class EvoScSolution : IEvoScSolution
{
    public required SolutionFile SolutionFile { get; set; }
    public required string SolutionFilePath { get; init; }
    
    public Task RefreshAsync()
    {
        SolutionFile = SolutionFile.Parse(SolutionFilePath);
        return Task.CompletedTask;
    }
}