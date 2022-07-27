using Common.Cli;
using Common.Maths;
using Common.Maths.Extension;

namespace LToolBox.Tools;

internal class PolynomialTool : Tool
{
    public override string ToolName { get; } = "polynomial";

    public override string? HelpMsg { get; } = "Calculate polynomial\nType `q` to quit";

    public override void Execute()
    {
        double a = 0, b = 0, c = 0;
        var pos = 0;
        var top = Console.CursorTop;
        ReRenderExp(a, b, c, pos);

        for (;;)
        {
            var input = Console.ReadKey(true);

            switch (input)
            {
                case { Key: ConsoleKey.LeftArrow }:
                case { Key: ConsoleKey.H }:
                {
                    if (pos is 0)
                        continue;

                    pos--;
                    break;
                }
                case { Key: ConsoleKey.RightArrow }:
                case { Key: ConsoleKey.L }:
                {
                    if (pos >= 2)
                        continue;

                    pos++;
                    break;
                }
                case { Key: ConsoleKey.D0 }:
                case { Key: ConsoleKey.D1 }:
                case { Key: ConsoleKey.D2 }:
                case { Key: ConsoleKey.D3 }:
                case { Key: ConsoleKey.D4 }:
                case { Key: ConsoleKey.D5 }:
                case { Key: ConsoleKey.D6 }:
                case { Key: ConsoleKey.D7 }:
                case { Key: ConsoleKey.D8 }:
                case { Key: ConsoleKey.D9 }:
                case { Key: ConsoleKey.NumPad0 }:
                case { Key: ConsoleKey.NumPad1 }:
                case { Key: ConsoleKey.NumPad2 }:
                case { Key: ConsoleKey.NumPad3 }:
                case { Key: ConsoleKey.NumPad4 }:
                case { Key: ConsoleKey.NumPad5 }:
                case { Key: ConsoleKey.NumPad6 }:
                case { Key: ConsoleKey.NumPad7 }:
                case { Key: ConsoleKey.NumPad8 }:
                case { Key: ConsoleKey.NumPad9 }:
                {
                    var val = ConsoleHelpers.PromptIntAndClearLine("Value: ", top + 1,
                        defaultValue: EnumHelpers.FastConsoleKeyToNumberString(input.Key));
                    Console.CursorTop = top;

                    if (!val.HasValue)
                        continue;

                    switch (pos)
                    {
                        case 0:
                        {
                            a = val.Value;
                            break;
                        }
                        case 1:
                        {
                            b = val.Value;
                            break;
                        }
                        default:
                        {
                            c = val.Value;
                            break;
                        }
                    }

                    if (pos < 2)
                        pos++;

                    break;
                }
                case { KeyChar: '-' }:
                {
                    var val = ConsoleHelpers.PromptIntAndClearLine("Value: ", top + 1, defaultValue: "-");
                    Console.CursorTop = top;

                    if (!val.HasValue)
                        continue;

                    switch (pos)
                    {
                        case 0:
                        {
                            a = val.Value;
                            break;
                        }
                        case 1:
                        {
                            b = val.Value;
                            break;
                        }
                        default:
                        {
                            c = val.Value;
                            break;
                        }
                    }

                    if (pos < 2)
                        pos++;

                    break;
                }
                case { Key: ConsoleKey.Q }:
                case { Key: ConsoleKey.Escape }:
                {
                    Console.WriteLine();
                    return;
                }
                case { Key: ConsoleKey.Enter }:
                {
                    if (a is 0 && b is 0 && c is 0)
                        continue;
                    break;
                }
                default:
                    continue;
            }

            if (input.Key is ConsoleKey.Enter)
                break;

            ReRenderExp(a, b, c, pos);
        }

        Console.WriteLine();
        Console.Write("Result: ");
        switch (Polynomial.Calc2(a, b, c, out var result1, out var result2))
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

    private static void ReRenderExp(double a, double b, double c, int pos)
    {
        Console.CursorLeft = 0;
        ConsoleHelpers.ClearLine();

        RenderVariable(a, 2, pos is 0);
        Console.Write(" + ");
        RenderVariable(b, 1, pos is 1);
        Console.Write(" + ");
        RenderVariable(c, 0, pos is 2);
    }

    private static void RenderVariable(double val, int power, bool isSelected)
    {
        if (isSelected)
            Console.ForegroundColor = ConsoleColor.Green;

        Console.Write(val);

        Console.ForegroundColor = ConsoleColor.Cyan;

        if (power <= 0)
        {
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        Console.Write('x');

        if (power <= 1)
        {
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        Console.Write('^');
        Console.Write(power);

        Console.ForegroundColor = ConsoleColor.White;
    }
}