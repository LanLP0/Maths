using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Common.Cli;
using Common.Maths;
using Common.Maths.Extension;
using Spectre.Console;

namespace LToolBox.Tools;

internal sealed class PolynomialTool : Tool
{
    public PolynomialTool(IAnsiConsole console) : base(console)
    {
    }

    public override string ToolName { get; } = "polynomial";

    public override string? HelpMsg { get; } = "Calculate polynomial\nPress [Yellow]q[/], [yellow]Esc[/] to exit";

    public override void Execute()
    {
        double a = 0, b = 0, c = 0;
        var pos = 0;
        RenderExp(a, b, c, pos);

        for (;;)
        {
            var input = Console.ReadKey(true)!.Value;

            switch (input)
            {
                case { Key: ConsoleKey.LeftArrow }:
                case { Key: ConsoleKey.H }: // Move left
                {
                    if (pos is 0)
                        continue;

                    pos--;
                    break;
                }
                case { Key: ConsoleKey.RightArrow }:
                case { Key: ConsoleKey.L }: // Move right
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
                case { Key: ConsoleKey.NumPad9 }: // Change the value
                {
                    var val = Console.Ask<int?>("Value:", true,
                        EnumHelpers.FastToString(input.Key));

                    if (!val.HasValue)
                        continue;

                    ChangeValueAndMoveNext(ref pos, val, ref a, ref b, ref c);

                    break;
                }
                case { KeyChar: '-' }: // Change the value
                {
                    var val = Console.Ask<int?>("Value:", true, "-");

                    if (!val.HasValue)
                        continue;

                    ChangeValueAndMoveNext(ref pos, val, ref a, ref b, ref c);

                    break;
                }
                case { Key: ConsoleKey.Q }:
                case { Key: ConsoleKey.Escape }: // Exit
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

            RenderExp(a, b, c, pos);
        }

        Console.WriteLine();
        Console.Markup("[white]Result:[/] ");
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

    private static void ChangeValueAndMoveNext(ref int pos, [DisallowNull] int? val, ref double a, ref double b,
        ref double c)
    {
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
    }

    private void RenderExp(double a, double b, double c, int pos)
    {
        Console.ClearLine();

        RenderVariable(a, 2, pos is 0);
        Console.Write(" + ");
        RenderVariable(b, 1, pos is 1);
        Console.Write(" + ");
        RenderVariable(c, 0, pos is 2);
    }

    private void RenderVariable(double val, int power, bool isSelected)
    {
        var style = Style.Plain;
        if (isSelected)
            style = style.Foreground(Color.Green);

        var markup = new Text(val.ToString(CultureInfo.InvariantCulture), style);
        Console.Write(markup);

        if (power <= 0) return;

        Console.Markup("[cyan]x[/]");

        if (power <= 1) return;

        Console.Markup($"[cyan]^{power}[/]");
    }
}