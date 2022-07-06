using Common;

namespace LToolBox.Tools;

public class Fact : Tool
{
    public override string ToolName { get; } = "fact";

    public override string? HelpMsg { get; } = "Find fact(s) of a number\nPress `q` to quit";

    public override void Execute()
    {
        var value = ConsoleHelpers.PromptInt("Number: ", null, 6, false);
        if (value is null)
            return;

        Console.Write("Result: ");
        Console.WriteLine(string.Join(" * ",  Common.Maths.Maths.GetFact(value.Value)));
    }
}