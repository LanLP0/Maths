using LToolBox;
using Spectre.Console.Cli;

Console.CancelKeyPress += (_, _) => Environment.Exit(0);

var app = new CommandApp<App>();
return app.Run(args);