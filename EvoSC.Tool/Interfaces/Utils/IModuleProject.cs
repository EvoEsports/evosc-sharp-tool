using Microsoft.Build.Construction;

namespace EvoSC.Tool.Interfaces.Utils;

public interface IModuleProject
{
    public string Name { get; }
    public string Title { get; }
    public string Description { get; }
    public string Author { get; }
    
    public Task GenerateAsync(IEvoScSolution solution, bool isInternal);
}