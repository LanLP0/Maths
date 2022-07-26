using Common.Cli;
using Common.Maths;

namespace LToolBox.Tools;

internal class FactTool : Tool
{
    public override string ToolName { get; } = "fact";

    public override string? HelpMsg { get; } = "Find fact(s) of a number\nPress `q` to quit";

    public override void Execute()
    {
        var value = ConsoleHelpers.PromptInt("Number: ", null, 9, false);
        if (value is null)
            return;

        Console.Write("Result: ");
        Console.WriteLine(string.Join(" * ", Maths.GetFact(value.Value)));
    }
}