using Common.Maths.Extension;
using LToolBox.Delta;

namespace LToolBox.Tools;

internal class DeltaTool : Tool
{
    public override string ToolName { get; } = "delta";

    public override string? HelpMsg { get; } = "Calculate a fraction using delta\nType `q` to exit";

    public override void Execute()
    {
        var top = Console.CursorTop;

        var deltaFraction = DeltaHelpers.PromptDelta(top);

        if (deltaFraction is null)
            return;

        var delta = deltaFraction.Calc();
        var finalDelta = delta.Calc();

        Console.SetCursorPosition(0, top + 3);
        DeltaHelpers.RenderFinalDelta(finalDelta);
        Console.SetCursorPosition(0, top + 4);

        Console.Write("Result: ");
        switch (finalDelta.Calc(out var result1, out var result2))
        {
            case -1:
            {
                Console.Write("Infinite results");
                break;
            }
            case 0:
            {
                Console.Write("No result");
                break;
            }
            case 1:
            {
                Console.Write(result1!.Value.ToFraction());
                break;
            }
            case 2:
            {
                Console.Write(result1!.Value.ToFraction());
                Console.Write(", ");
                Console.Write(result2!.Value.ToFraction());
                break;
            }
        }

        Console.WriteLine();
    }
}