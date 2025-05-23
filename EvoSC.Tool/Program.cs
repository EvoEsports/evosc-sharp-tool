﻿using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using EvoSC.Tool;
using EvoSC.Tool.Middlewares;
using EvoSC.Tool.Utils;
using Microsoft.Build.Locator;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

MSBuildLocator.RegisterDefaults();

var builder = new CommandLineBuilder(SetupCommands.Setup());


builder.UseDefaults();

builder.AddEvoSCProjectCheck();
builder.AddDependencyInjection(services =>
{
    services.AddSingleton(AnsiConsole.Console);
});

var app = builder.Build();

await app.InvokeAsync(args);
