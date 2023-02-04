using System.Text;
using Common.Cli;
using LToolBox.Tools;
#if DEBUG
using Serilog;
#endif

namespace LToolBox;

internal sealed class Program
{
    private static Tool[] _tools = null!;

    private static string _promptTemplate = null!;

#if DEBUG
    public static readonly ILogger Logger = new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .WriteTo.Console()
        .CreateLogger();
#endif

    public static void Main()
    {
        Console.CancelKeyPress += (_, _) =>
        {
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(0);
        };
        _tools = new Tool[]
        {
            new ComplexCalcTool(
#if DEBUG
                Logger
#endif
            ),
            new LCalcTool(),
            new MinMaxFracTool(),
            new PolynomialTool(),
            new FactTool(),
            new IsPrimeTool()
        };

        StringBuilder buffer = new();
        buffer.Append("> ");
        var count = 1;
        foreach (var tool in _tools)
        {
            buffer.Append(count);
            buffer.Append('-');
            buffer.Append(tool.ToolName);
            buffer.Append(' ');

            count++;
        }

        buffer.Append("\n> Select tool: ");
        _promptTemplate = buffer.ToString();
        buffer.Clear();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("> Press `?` to get help about a tool, press <Ctrl-C> to exit");

        for (;;) PromptTool();
    }

    private static void PromptTool()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(_promptTemplate);
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
                case -2:
                {
                    Console.CursorLeft = 0;
                    ConsoleHelpers.ClearLine();

                    Console.Write("> Help: ");
                    help = true;

                    continue;
                }
                case -1:
                {
                    continue;
                }
            }

            if (id >= _tools.Length)
                continue;

            Console.ForegroundColor = ConsoleColor.White;

            var tool = _tools.ElementAt(id);
            Console.WriteLine(tool.ToolName);

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