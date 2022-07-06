using LCalc;

namespace LToolBox.Tools;

internal class LCalc : Tool
{
    public override string ToolName { get; } = "lcalc";

    public override string? HelpMsg { get; } = "A calculator\nType `q` to exit";

    public override void Execute()
    {
        for (;;)
        {
            Console.Write("Expression: ");
            var input = Console.ReadLine()!;
            
            if (input.Length is 0 && input is "q")
                return;
            
            Console.WriteLine(Calculator.Calc(input));
        }
    }
}