using LToolBox.Tools;
using System.Text;
using Common.Cli;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace LToolBox;

class Program
{
    [NotNull]
    private static Tool[] _tools;
    [NotNull]
    private static string _promptTemplate;

    public static void Main()
    {
        Console.CancelKeyPress += (_, _) =>
        {
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(0);
        };

        var serviceProvider = new ServiceCollection()
#if DEBUG
            .AddLogging(a =>
            {
                var logger = new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .WriteTo.Console();
                
                a.AddSerilog(logger.CreateLogger());
            })
#endif
            .AddSingleton<Tool, ComplexMultiplyTool>()
            .AddSingleton<Tool, Tools.DeltaTool>()
            .AddSingleton<Tool, Tools.LCalcTool>()
            .AddSingleton<Tool, PolynomialTool>()
            .AddSingleton<Tool, FactTool>()
            .AddSingleton<Tool, IsPrimeTool>()
            .BuildServiceProvider();

        _tools = serviceProvider.GetServices<Tool>().ToArray();

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
        
        for (;;)
        {
            PromptTool();
        }
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