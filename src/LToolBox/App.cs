using System.ComponentModel;
using System.Text;
using Common.Cli;
using LToolBox.Tools;
using Spectre.Console;
using Spectre.Console.Cli;
#if DEBUG
using Serilog;
#endif

namespace LToolBox;

internal sealed class App : Command<Settings>
{
    private static Tool[] _tools = null!;

    private static string _prompt = null!;

#if DEBUG
    public static readonly ILogger Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Console()
        .CreateLogger();
#endif

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!settings.Focus)
            return ExecuteCore(settings);

        Console.CancelKeyPress += (_, e) => Environment.Exit(0);

        if (!AnsiConsole.Profile.Capabilities.AlternateBuffer)
        {
            AnsiConsole.Clear();
            return ExecuteCore(settings);
        }

        var exitCode = 0;
        AnsiConsole.AlternateScreen(() =>
        {
            exitCode = ExecuteCore(settings);
        });

        return exitCode;
    }
    
    private int ExecuteCore(Settings settings)
    {
        var console = AnsiConsole.Console;

        _tools = new Tool[]
        {
            new ComplexCalcTool(console),
            new LCalcTool(console),
            new MinMaxFracTool(console),
            new PolynomialTool(console),
            new FactTool(console),
            new IsPrimeTool(console)
        };

        StringBuilder buffer = new();
        buffer.Append("[Green]>[/] ");
        var count = 1;
        foreach (var tool in _tools)
        {
            buffer.Append("[Green]");
            buffer.Append(count);
            buffer.Append("[/]");
            buffer.Append('-');
            buffer.Append("[Yellow]");
            buffer.Append(tool.ToolName);
            buffer.Append("[/]");
            buffer.Append(' ');

            count++;
        }

        buffer.Append("\n[Green]>[/] [white]Select tool:[/] ");
        _prompt = buffer.ToString();
        buffer.Clear();

        if (settings.ToolId.HasValue)
        {
            var c = settings.ToolId.Value;
            // ReSharper disable once RedundantCast
            var id = (int)c switch
            {
                > 48 and < 58 => c - 49,
                > 96 and < 123 => c - 88,
                _ => -1
            };

            if (id is -1 || id >= _tools.Length)
            {
                AnsiConsole.MarkupLine("[red]> Invalid tool id[/]");
            }
            else
            {
                var tool = _tools[id];
                AnsiConsole.MarkupLine("[green]>[/] Tool: [white]{0}[/]", tool.ToolName);

                tool.Execute();
            }

            if (settings.Quit)
                return 0;
        }

        AnsiConsole.MarkupLine(
            "[Green]>[/] Type [Yellow]?[/] to get help about a tool, press [Yellow]Ctrl-C[/] to exit");

        if (settings.Quit)
        {
            PromptToolAndRun();
            return 0;
        }

        for (;;)
        {
            PromptToolAndRun();
            if (!settings.Focus)
                continue;

            AnsiConsole.Console.ReadKey(true);
            AnsiConsole.Clear();
        }
    }

    private static void PromptToolAndRun()
    {
        AnsiConsole.Markup(_prompt);
        var help = false;

        for (;;)
        {
            var input = Console.ReadKey(true);

            var id = (int)input.KeyChar switch
            {
                63 => -2,
                > 48 and < 58 => input.KeyChar - 49,
                > 96 and < 123 => input.KeyChar - 88,
                _ => -1
            };

            switch (id)
            {
                case -2: // Help key
                {
                    AnsiConsole.Console.ClearLine();

                    AnsiConsole.Markup("[Green]>[/] [white]Help:[/] ");
                    help = true;

                    continue;
                }
                case -1: // Unknown key
                {
                    continue;
                }
            }

            if (id >= _tools.Length)
                continue;

            var tool = _tools[id];
            AnsiConsole.WriteLine(tool.ToolName);

            if (help)
            {
                tool.ExecuteHelp();
                return;
            }

            tool.Execute();
            return;
        }
    }
}

public sealed class Settings : CommandSettings
{
    [Description("The tool to launch after startup")]
    [CommandOption("-t|--tool-id")]
    public char? ToolId { get; init; }

    [Description("Quit after the first tool ran")]
    [CommandOption("-q|--quit")]
    [DefaultValue(false)]
    public bool Quit { get; init; }
    
    [Description("Clear the console")]
    [CommandOption("-f|--focus")]
    [DefaultValue(false)]
    public bool Focus { get; init; }
}