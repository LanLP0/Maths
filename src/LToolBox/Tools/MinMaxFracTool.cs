using System.Diagnostics.CodeAnalysis;
using System.Text;
using Common.Cli;
using MinMaxFraction.Core;
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
        {
            Console.WriteLine();
            return;
        }

        var polynomial = fraction.Calc();
        var deltaResult = polynomial.Calc();

        RenderDeltaResult(deltaResult);

        Console.Markup("[white]Result:[/] ");
        Console.Write(deltaResult.RenderResult());

        Console.WriteLine();
    }

    public MMFraction? PromptFraction()
    {
        var isUpper = true;
        var pos = 0;
        var fraction = new MMFraction
        {
            T0 = 0,
            T1 = 0,
            T2 = 0,
            B0 = 0,
            B1 = 0,
            B2 = 0
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
                case { Key: ConsoleKey.H }: // Move previous
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
                case { Key: ConsoleKey.L }: // Move next
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

    private static void ChangeValueAndMoveNext(ref bool isUpper, MMFraction fraction, [DisallowNull] int? val,
        ref int pos)
    {
        if (isUpper)
        {
            switch (pos)
            {
                case 0:
                {
                    fraction.T0 = val.Value;
                    break;
                }
                case 1:
                {
                    fraction.T1 = val.Value;
                    break;
                }
                default:
                {
                    fraction.T2 = val.Value;
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
                fraction.B0 = val.Value;
                break;
            }
            case 1:
            {
                fraction.B1 = val.Value;
                break;
            }
            default:
            {
                fraction.B2 = val.Value;
                break;
            }
        }

        if (pos < 2)
            pos++;
    }

    private void RenderFraction(MMFraction fraction, bool isUpper, int pos)
    {
        // Clear space for the fraction and set the cursor to the right place
        Console.ClearLine();
        Console.Cursor.MoveUp();
        Console.ClearLine();
        Console.Cursor.MoveUp();
        Console.ClearLine();

        StringBuilder topBuffer = new();
        StringBuilder bottomBuffer = new();

        RenderTopBottom(fraction, isUpper, pos, topBuffer, bottomBuffer);

        // Calculate the real size by subtraction the markups
        var topBufferRealLength = topBuffer.Length - 18 - (isUpper ? 10 : 0);
        var bottomBufferRealLength = bottomBuffer.Length - 18 - (isUpper ? 0 : 10);

        int dashCharCount;
        if (topBufferRealLength >= bottomBufferRealLength)
        {
            dashCharCount = topBufferRealLength + 2;

            if (topBufferRealLength != bottomBufferRealLength)
            {
                var offset = (topBufferRealLength - bottomBufferRealLength) / 2;
                bottomBuffer.Insert(0, new string(' ', offset));
            }
        }
        else
        {
            dashCharCount = bottomBufferRealLength + 2;

            var offset = (bottomBufferRealLength - topBufferRealLength) / 2;
            topBuffer.Insert(0, new string(' ', offset));
        }

        topBuffer.Insert(0, "      ");

        topBuffer.Append("\n A = ");
        topBuffer.Append(new string('-', dashCharCount));

        topBuffer.Append("\n      ");
        topBuffer.Append(bottomBuffer);

        Console.Markup(topBuffer.ToString());
        Console.MoveCursorToStart();
    }

    private static void RenderTopBottom(MMFraction fraction, bool isUpper, int pos, StringBuilder topBuffer,
        StringBuilder bottomBuffer)
    {
        if (isUpper)
        {
            RenderVariableToBuffer(fraction.T0, pos is 0, topBuffer);
            topBuffer.Append("[Cyan]x^2[/] + ");
            RenderVariableToBuffer(fraction.T1, pos is 1, topBuffer);
            topBuffer.Append("[Cyan]x[/] + ");
            RenderVariableToBuffer(fraction.T2, pos is 2, topBuffer);
            RenderVariableToBuffer(fraction.B0, bottomBuffer);
            bottomBuffer.Append("[Cyan]x^2[/] + ");
            RenderVariableToBuffer(fraction.B1, bottomBuffer);
            bottomBuffer.Append("[Cyan]x[/] + ");
            RenderVariableToBuffer(fraction.B2, bottomBuffer);
            return;
        }

        RenderVariableToBuffer(fraction.T0, topBuffer);
        topBuffer.Append("[Cyan]x^2[/] + ");
        RenderVariableToBuffer(fraction.T1, topBuffer);
        topBuffer.Append("[Cyan]x[/] + ");
        RenderVariableToBuffer(fraction.T2, topBuffer);
        RenderVariableToBuffer(fraction.B0, pos is 0, bottomBuffer);
        bottomBuffer.Append("[Cyan]x^2[/] + ");
        RenderVariableToBuffer(fraction.B1, pos is 1, bottomBuffer);
        bottomBuffer.Append("[Cyan]x[/] + ");
        RenderVariableToBuffer(fraction.B2, pos is 2, bottomBuffer);
    }

    public void RenderDeltaResult(MMDeltaResult mmDeltaResult)
    {
        var strBuilder = new StringBuilder();

        RenderVariableToBuffer(mmDeltaResult.V0, strBuilder);
        strBuilder.Append("[Cyan]A^2[/] + ");
        RenderVariableToBuffer(mmDeltaResult.V1, strBuilder);
        strBuilder.Append("[Cyan]A[/] + ");
        RenderVariableToBuffer(mmDeltaResult.V2, strBuilder);

        Console.MarkupLine(strBuilder.ToString());
    }

    private static void RenderVariableToBuffer(double variable, StringBuilder buffer)
    {
        RenderVariableToBuffer(variable, false, buffer);
    }

    private static void RenderVariableToBuffer(double variable, bool isSelected, StringBuilder buffer)
    {
        if (isSelected)
            buffer.Append("[Green]");

        buffer.Append(variable);

        if (isSelected)
            buffer.Append("[/]");
    }
}