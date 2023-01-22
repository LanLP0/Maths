using LCalc;

namespace LToolBox.Tools;

internal sealed class LCalcTool : Tool
{
    public override string ToolName { get; } = "lcalc";

    public override string? HelpMsg { get; } = "A powerful calculator\nType `q` to exit";

    public override void Execute()
    {
        for (;;)
        {
            Console.Write("Expression: ");
            var input = Console.ReadLine()!;

            if (input.Length is 1 && input is "q")
                return;

            Console.WriteLine(Calculator.CalcFormatted(input));
        }
    }
}