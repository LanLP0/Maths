using Common.Cli;
using Common.Maths.Extension;
using LToolBox.Delta;

namespace LToolBox.Tools;

internal sealed class MinMaxFracTool : Tool
{
    public override string ToolName { get; } = "minmaxfrac";

    public override string? HelpMsg { get; } =
        "Calculate minimum and maximum value of a fraction\nUse arrow keys or vim keys to move around\nEnter value by start typing it\nType `q` to exit";

    public override void Execute()
    {
        var top = Console.CursorTop;
        var (deltaFraction, afterTop) = DeltaHelpers.PromptDelta(top);

        if (deltaFraction is null)
            return;

        top = afterTop;

        var delta = deltaFraction.Calc();
        var finalDelta = delta.Calc();

        ConsoleHelpers.SafeSetCursorPosition(0, top + 3);
        DeltaHelpers.RenderFinalDelta(finalDelta);

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
                Console.Write(result1!.Value.Humanize());
                break;
            }
            case 2:
            {
                Console.Write(result1!.Value.Humanize());
                Console.Write(", ");
                Console.Write(result2!.Value.Humanize());
                break;
            }
        }

        Console.WriteLine();
    }
}