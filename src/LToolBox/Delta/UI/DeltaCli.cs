namespace LToolBox.Delta.UI;

internal class DeltaCli
{
    public static void Execute()
    {
        // Console.Write(new string('a', Console.WindowWidth));
        // Console.Write(new string('a', Console.WindowWidth));
        // Console.Write(new string('a', Console.WindowWidth));
        // Console.SetCursorPosition(0, 1);
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
                Console.Write(result1!.Value);
                break;
            }
            case 2:
            {
                Console.Write(result1!.Value);
                Console.Write(", ");
                Console.Write(result2!.Value);
                break;
            }
        }

        Console.WriteLine();
    }
}