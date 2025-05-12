using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Reflection;
using EvoSC.Tool.Commands;
using EvoSC.Tool.Interfaces.Utils;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace EvoSC.Tool.Utils;

public abstract class ToolCommand<TOptions, THandler> : Command
where TOptions : class, IToolCommandOptions
where THandler : class, IToolCommandHandler<TOptions>
{
    protected ToolCommand(string name, string? description = null) : base(name, description)
    {
        var optionsInstance = Activator.CreateInstance<TOptions>();
        optionsInstance.AddOptions(this);
        
        Handler = CommandHandler.Create<TOptions, IServiceProvider>((options, services) =>
        {
            var handlerInstance = ActivatorUtilities.CreateInstance<THandler>(services);
            handlerInstance.SetConsole(services.GetRequiredService<IAnsiConsole>());
            return handlerInstance.ExecuteAsync(options);
        });
    }
}
