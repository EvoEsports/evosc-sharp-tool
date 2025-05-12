
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace EvoSC.Tool.Utils;

public class SpectreILRepackConsoleLogger(IAnsiConsole console, string name) : ILRepacking.ILogger
{
    public void Error(string msg) => LogMultiline(LogLevel.Error, msg);

    public void Warn(string msg) => LogMultiline(LogLevel.Warning, msg);

    public void Info(string msg) => LogMultiline(LogLevel.Information, msg);

    public void Verbose(string msg) => LogMultiline(LogLevel.Debug, msg);

    public bool ShouldLogVerbose { get; set; }

    public void LogMultiline(LogLevel logLevel, string msg)
    {
        var lines = msg.Split(Environment.NewLine);
        foreach (var line in lines)
        {
            Log(logLevel, line);
        }
    }
    
    public void Log(LogLevel logLevel, string message)
    {
        if (!ShouldLogVerbose && logLevel == LogLevel.Debug)
        {
            return;
        }
        
        var levelMarkup = logLevel switch
        {
            LogLevel.Debug => "[bold gray]dbug[/]",
            LogLevel.Information => "[bold white]info[/]",
            LogLevel.Warning => "[bold yellow]warn[/]",
            LogLevel.Error => "[bold red]fail[/]",
            _ => "[gray]"
        };
        
        var msgMarkup = $"[{(logLevel == LogLevel.Error ? "bold white" : "white")}]{message}[/]";
        var typeMarkup = $"[italic teal]{name}[/]";
        var messageMarkup = $"{typeMarkup} {levelMarkup} {msgMarkup}";

        console.MarkupLine(messageMarkup);
    }
}