using System.Text;
using Common.Cli;
using LToolBox.Tools;
using Spectre.Console;
#if DEBUG
using Serilog;
#endif

namespace LToolBox;

internal sealed class Program
{
    private static Tool[] _tools = null!;

    private static string _prompt = null!;

#if DEBUG
    public static readonly ILogger Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Console()
        .CreateLogger();
#endif

    public static void Main()
    {
        if (!AnsiConsole.Profile.Capabilities.Interactive)
            AnsiConsole.MarkupLine("[red]This program needs to be run in interactive mode[/]");

        Console.CancelKeyPress += (_, _) => { Environment.Exit(0); };

        var console = AnsiConsole.Console;

        _tools = new Tool[]
        {
            new ComplexCalcTool(
                console
#if DEBUG
                , Logger
#endif
            ),
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

        buffer.Append("\n[Green]>[/] Select tool: ");
        _prompt = buffer.ToString();
        buffer.Clear();

        AnsiConsole.MarkupLine(
            "[Green]>[/] Type [Yellow]?[/] to get help about a tool, press [Yellow]Ctrl-C[/] to exit");

        for (;;) PromptTool();
    }

    private static void PromptTool()
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

                    AnsiConsole.Markup("[Green]>[/] Help: ");
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

            var tool = _tools.ElementAt(id);
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