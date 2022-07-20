using Common.Cli;

namespace LToolBox.Tools;

internal class FactTool : Tool
{
    public override string ToolName { get; } = "fact";

    public override string? HelpMsg { get; } = "Find fact(s) of a number\nPress `q` to quit";

    public override void Execute()
    {
        var value = ConsoleHelpers.PromptInt("Number: ", null, 8, false);
        if (value is null)
            return;

        Console.Write("Result: ");
        Console.WriteLine(string.Join(" * ",  Common.Cli.Maths.Maths.GetFact(value.Value)));
    }
}