using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
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
        int degree;
        Console.Markup("Degree [blue](2-5)[/]: ");
        for (;;)
        {
            var input = Console.ReadKey(true)!.Value.KeyChar;
            if (input is < '2' or > '5')
                continue;
            
            degree = input - '2' + 2;
            break;
        }
        Console.WriteLine();

        // {5, 0, 2} -> "p : x -> 5 + 0 x^1 + 2 x^2"
        var coefficients = new double[degree + 1];
        var pos = degree;
        RenderExp(coefficients, pos);

        for (;;)
        {
            var input = Console.ReadKey(true)!.Value;

            switch (input)
            {
                case { Key: ConsoleKey.RightArrow }:
                case { Key: ConsoleKey.L }: // Move right
                {
                    if (pos is 0)
                        continue;

                    pos--;
                    break;
                }
                case { Key: ConsoleKey.LeftArrow }:
                case { Key: ConsoleKey.H }: // Move left
                {
                    if (pos >= coefficients.Length)
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

                    ChangeValueAndMoveNext(ref pos, val.Value, coefficients);

                    break;
                }
                case { KeyChar: '-' }: // Change the value
                {
                    var val = Console.Ask<int?>("Value:", true, "-");

                    if (!val.HasValue)
                        continue;

                    ChangeValueAndMoveNext(ref pos, val.Value, coefficients);

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
                    if (coefficients.All(x => x is 0))
                        continue;

                    goto BreakLoop;
                }
                default:
                    continue;
            }

            RenderExp(coefficients, pos);
        }
        
        BreakLoop:
        
        Console.WriteLine();
        Console.Markup("[white]Result:[/] ");

        var roots = ComputeRoot(coefficients);

        var resultText = string.Join(", ", roots.Select(root => root.Humanize()));
        Console.WriteLine(resultText);
    }

    private Complex[] ComputeRoot(double[] coefficients)
    {
        var polynomial = new MathNet.Numerics.Polynomial(coefficients);
        return polynomial.Roots();
    }

    private static void ChangeValueAndMoveNext(ref int pos, int val, double[] coefficients)
    {
        coefficients[pos] = val;

        if (pos > 0)
            pos--;
    }

    private void RenderExp(double[] coefficients, int pos)
    {
        Console.ClearLine();

        for (var i = coefficients.Length - 1; i > 0; i--)
        {
            RenderVariable(coefficients[i], i, pos == i);
            Console.Write(" + ");
        }

        RenderVariable(coefficients[0], 0, pos == 0);
        Console.Write(" = 0");
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