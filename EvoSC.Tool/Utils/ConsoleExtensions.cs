
using ILRepacking;
using Spectre.Console;

namespace EvoSC.Tool.Utils;

public static class ConsoleExtensions
{
    public static ILogger CreateILRepackLogger(this IAnsiConsole console) => new SpectreILRepackConsoleLogger(console, "ILRepack");
}