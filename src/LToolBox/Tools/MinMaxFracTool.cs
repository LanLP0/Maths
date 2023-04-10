using System.Diagnostics.CodeAnalysis;
using System.Text;
using Common.Cli;
using Common.Maths.Extension;
using Delta.Core;
using Spectre.Console;

namespace LToolBox.Tools;

internal sealed class MinMaxFracTool : Tool
{
    public MinMaxFracTool(IAnsiConsole console) : base(console)
    {
    }

    public override string ToolName { get; } = "minmaxfrac";

    public override string? HelpMsg { get; } = """
        Calculate minimum and maximum value of a fraction
        Use [Yellow]arrow keys[/] or [Yellow]vim keys[/] to move around
        Enter value by start typing it
        Type [Yellow]q[/] to exit
        """;

    public override void Execute()
    {
        Console.WriteLine();
        Console.WriteLine();

        var fraction = PromptFraction();

        if (fraction is null)
            return;

        var delta = fraction.Calc();
        var finalDelta = delta.Calc();

        RenderDelta(finalDelta);

        Console.Markup("[white]Result:[/] ");
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

    public DeltaFraction? PromptFraction()
    {
        var isUpper = true;
        var pos = 0;
        var fraction = new DeltaFraction
        {
            N0 = { NumberPart = 0 },
            N1 = { NumberPart = 0 },
            N2 = { NumberPart = 0 },
            D0 = { NumberPart = 0 },
            D1 = { NumberPart = 0 },
            D2 = { NumberPart = 0 }
        };

        RenderFraction(fraction, isUpper, pos);

        for (;;)
        {
            var input = Console.ReadKey(true)!.Value;

            switch (input)
            {
                case { Key: ConsoleKey.DownArrow }:
                case { Key: ConsoleKey.J }: // Move down
                {
                    if (!isUpper)
                        break;

                    isUpper = false;
                    break;
                }
                case { Key: ConsoleKey.UpArrow }:
                case { Key: ConsoleKey.K }: // Move up
                {
                    if (isUpper)
                        break;

                    isUpper = true;
                    break;
                }
                case { Key: ConsoleKey.LeftArrow }:
                case { Key: ConsoleKey.H }: // Move next
                {
                    if (pos is 0)
                    {
                        isUpper = !isUpper;
                        pos = 2;
                        break;
                    }

                    pos--;
                    break;
                }
                case { Key: ConsoleKey.RightArrow }:
                case { Key: ConsoleKey.L }: // Move previous
                {
                    if (pos >= 2)
                    {
                        isUpper = !isUpper;
                        pos = 0;
                        break;
                    }

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
                case { Key: ConsoleKey.NumPad9 }: // Change value
                {
                    var val = Console.Ask<int?>("Value:", true,
                        EnumHelpers.FastToString(input.Key));

                    if (!val.HasValue)
                        break;

                    ChangeValueAndMoveNext(ref isUpper, fraction, val, ref pos);

                    break;
                }
                case { KeyChar: '-' }: // Change value
                {
                    var val = Console.Ask<int?>("Value: ", true, "-");

                    if (!val.HasValue)
                        break;

                    ChangeValueAndMoveNext(ref isUpper, fraction, val, ref pos);

                    break;
                }
                case { Key: ConsoleKey.Q }:
                case { Key: ConsoleKey.Escape }: // Exit
                {
                    return null;
                }
                case { Key: ConsoleKey.Enter }:
                {
                    if (!fraction.Validate())
                        break;

                    Console.WriteLine();
                    return fraction;
                }
                default:
                    continue;
            }

            RenderFraction(fraction, isUpper, pos);
        }
    }

    private static void ChangeValueAndMoveNext(ref bool isUpper, DeltaFraction deltaFraction, [DisallowNull] int? val,
        ref int pos)
    {
        if (isUpper)
        {
            switch (pos)
            {
                case 0:
                {
                    deltaFraction.N0.NumberPart = val.Value;
                    break;
                }
                case 1:
                {
                    deltaFraction.N1.NumberPart = val.Value;
                    break;
                }
                default:
                {
                    deltaFraction.N2.NumberPart = val.Value;
                    break;
                }
            }

            // Move to the next value
            if (pos < 2)
            {
                pos++;
            }
            else
            {
                pos = 0;
                isUpper = false;
            }

            return;
        }

        switch (pos)
        {
            case 0:
            {
                deltaFraction.D0.NumberPart = val.Value;
                break;
            }
            case 1:
            {
                deltaFraction.D1.NumberPart = val.Value;
                break;
            }
            default:
            {
                deltaFraction.D2.NumberPart = val.Value;
                break;
            }
        }

        if (pos < 2)
            pos++;
    }

    private void RenderFraction(DeltaFraction deltaFraction, bool isUpper, int pos)
    {
        // Clear space for the fraction and set the cursor to the right place
        Console.ClearLine();
        Console.Cursor.MoveUp();
        Console.ClearLine();
        Console.Cursor.MoveUp();
        Console.ClearLine();

        StringBuilder nomBuffer = new();
        StringBuilder denomBuffer = new();

        RenderPart(deltaFraction, isUpper, pos, nomBuffer, denomBuffer);

        // Calculate the real size by subtraction the markups
        var nomBufferRealLength = nomBuffer.Length - 18 - (isUpper ? 10 : 0);
        var denomBufferRealLength = denomBuffer.Length - 18 - (isUpper ? 0 : 10);

        int dashCharsCount;
        if (nomBufferRealLength >= denomBufferRealLength)
        {
            dashCharsCount = nomBufferRealLength + 2;

            if (nomBufferRealLength != denomBufferRealLength)
            {
                var offset = (nomBufferRealLength - denomBufferRealLength) / 2;
                denomBuffer.Insert(0, new string(' ', offset));
            }
        }
        else
        {
            dashCharsCount = denomBufferRealLength + 2;

            var offset = (denomBufferRealLength - nomBufferRealLength) / 2;
            nomBuffer.Insert(0, new string(' ', offset));
        }

        nomBuffer.Insert(0, "      ");

        nomBuffer.Append("\n A = ");
        nomBuffer.Append(new string('-', dashCharsCount));

        nomBuffer.Append("\n      ");
        nomBuffer.Append(denomBuffer);

        Console.Markup(nomBuffer.ToString());
        Console.MoveCursorToStart();
    }

    private static void RenderPart(DeltaFraction fraction, bool isUpper, int pos, StringBuilder nomBuffer,
        StringBuilder denomBuffer)
    {
        if (isUpper)
        {
            RenderSimpleVariableToBuffer(fraction.N0, pos is 0, nomBuffer);
            nomBuffer.Append("[Cyan]x^2[/] + ");
            RenderSimpleVariableToBuffer(fraction.N1, pos is 1, nomBuffer);
            nomBuffer.Append("[Cyan]x[/] + ");
            RenderSimpleVariableToBuffer(fraction.N2, pos is 2, nomBuffer);
            RenderSimpleVariableToBuffer(fraction.D0, denomBuffer);
            denomBuffer.Append("[Cyan]x^2[/] + ");
            RenderSimpleVariableToBuffer(fraction.D1, denomBuffer);
            denomBuffer.Append("[Cyan]x[/] + ");
            RenderSimpleVariableToBuffer(fraction.D2, denomBuffer);
            return;
        }

        RenderSimpleVariableToBuffer(fraction.N0, nomBuffer);
        nomBuffer.Append("[Cyan]x^2[/] + ");
        RenderSimpleVariableToBuffer(fraction.N1, nomBuffer);
        nomBuffer.Append("[Cyan]x[/] + ");
        RenderSimpleVariableToBuffer(fraction.N2, nomBuffer);
        RenderSimpleVariableToBuffer(fraction.D0, pos is 0, denomBuffer);
        denomBuffer.Append("[Cyan]x^2[/] + ");
        RenderSimpleVariableToBuffer(fraction.D1, pos is 1, denomBuffer);
        denomBuffer.Append("[Cyan]x[/] + ");
        RenderSimpleVariableToBuffer(fraction.D2, pos is 2, denomBuffer);
    }

    public void RenderDelta(FinalDelta finalDelta)
    {
        var strBuilder = new StringBuilder();

        RenderSimpleVariableToBuffer(finalDelta.T0, strBuilder);
        strBuilder.Append("[Cyan]A^2[/] + ");
        RenderSimpleVariableToBuffer(finalDelta.T1, strBuilder);
        strBuilder.Append("[Cyan]A[/] + ");
        RenderSimpleVariableToBuffer(finalDelta.T2, strBuilder);

        Console.MarkupLine(strBuilder.ToString());
    }

    private static void RenderSimpleVariableToBuffer(SimpleVariable simpleVariable, StringBuilder buffer)
    {
        RenderSimpleVariableToBuffer(simpleVariable, false, buffer);
    }

    private static void RenderSimpleVariableToBuffer(SimpleVariable simpleVariable, bool isSelected,
        StringBuilder buffer)
    {
        if (isSelected)
            buffer.Append("[Green]");

        buffer.Append(simpleVariable.NumberPart);

        if (isSelected)
            buffer.Append("[/]");
    }
}