using Common;
using Common.Maths;

namespace LToolBox.Tools;

public class IsPrime : Tool
{
    public override string ToolName { get; } = "isprime";

    public override string? HelpMsg { get; } = "Check is a number is prime or not\nPress `q` to quit";

    public override void Execute()
    {
        var value = ConsoleHelpers.PromptInt("Number: ", null, 6, false);
        if (value is null)
            return;
        
        Console.Write("Result: ");
        Console.WriteLine(Maths.IsPrime(value.Value));
    }
}