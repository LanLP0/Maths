using System.Text;
using LCalc;
using LCalc.Cli;
using Spectre.Console;

namespace LToolBox.Tools;

internal sealed class LCalcTool : Tool
{
    private readonly StringBuilder _buffer = new();

    public LCalcTool(IAnsiConsole console) : base(console)
    {
    }

    public override string ToolName { get; } = "lcalc";

    public override string? HelpMsg { get; } = "A powerful calculator\nType [Yellow]q[/] to exit";

    public override void Execute()
    {
        Cli.RunLoop();
    }

    private static string GetResultText(CalcResult result)
    {
        if (result.Faulted)
            return "Error:";

        return "Result:";
    }
}