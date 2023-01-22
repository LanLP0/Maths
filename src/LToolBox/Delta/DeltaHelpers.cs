using System.Text;
using Common.Cli;
using Delta.Core;

namespace LToolBox.Delta;

internal static class DeltaHelpers
{
    public static (DeltaFraction?, int top) PromptDelta(int? top = null)
    {
        top ??= Console.CursorTop;

        top += ConsoleHelpers.SafeSetCursorPosition(0, top.Value + 3);

        var isUpper = true;
        var pos = 0;
        var deltaFraction = new DeltaFraction
        {
            T0 = { NumberPart = 0 },
            T1 = { NumberPart = 0 },
            T2 = { NumberPart = 0 },
            B0 = { NumberPart = 0 },
            B1 = { NumberPart = 0 },
            B2 = { NumberPart = 0 }
        };

        RenderDeltaFraction(deltaFraction, isUpper, pos, top);

        for (;;)
        {
            var input = Console.ReadKey(true);

            switch (input)
            {
                case { Key: ConsoleKey.DownArrow }:
                case { Key: ConsoleKey.J }:
                {
                    if (!isUpper)
                        break;

                    isUpper = false;
                    break;
                }
                case { Key: ConsoleKey.UpArrow }:
                case { Key: ConsoleKey.K }:
                {
                    if (isUpper)
                        break;

                    isUpper = true;
                    break;
                }
                case { Key: ConsoleKey.LeftArrow }:
                case { Key: ConsoleKey.H }:
                {
                    if (pos is 0)
                        break;

                    pos--;
                    break;
                }
                case { Key: ConsoleKey.RightArrow }:
                case { Key: ConsoleKey.L }:
                {
                    if (pos >= 2)
                        break;

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
                    var (val, adj) = ConsoleHelpers.PromptIntAndClearLine("Value: ", top.Value + 3,
                        defaultValue: EnumHelpers.FastConsoleKeyToNumberString(input.Key));

                    top += adj;

                    if (!val.HasValue)
                        break;

                    if (isUpper)
                    {
                        switch (pos)
                        {
                            case 0:
                            {
                                deltaFraction.T0.NumberPart = val.Value;
                                break;
                            }
                            case 1:
                            {
                                deltaFraction.T1.NumberPart = val.Value;
                                break;
                            }
                            default:
                            {
                                deltaFraction.T2.NumberPart = val.Value;
                                break;
                            }
                        }

                        if (pos < 2)
                        {
                            pos++;
                        }
                        else
                        {
                            pos = 0;
                            isUpper = false;
                        }
                    }
                    else
                    {
                        switch (pos)
                        {
                            case 0:
                            {
                                deltaFraction.B0.NumberPart = val.Value;
                                break;
                            }
                            case 1:
                            {
                                deltaFraction.B1.NumberPart = val.Value;
                                break;
                            }
                            default:
                            {
                                deltaFraction.B2.NumberPart = val.Value;
                                break;
                            }
                        }

                        if (pos < 2)
                            pos++;
                    }

                    break;
                }
                case { KeyChar: '-' }:
                {
                    var (val, adj) = ConsoleHelpers.PromptIntAndClearLine("Value: ", top + 3, defaultValue: "-");

                    top += adj;

                    if (!val.HasValue)
                        break;

                    if (isUpper)
                    {
                        switch (pos)
                        {
                            case 0:
                            {
                                deltaFraction.T0.NumberPart = val.Value;
                                break;
                            }
                            case 1:
                            {
                                deltaFraction.T1.NumberPart = val.Value;
                                break;
                            }
                            default:
                            {
                                deltaFraction.T2.NumberPart = val.Value;
                                break;
                            }
                        }

                        if (pos < 2)
                        {
                            pos++;
                        }
                        else
                        {
                            pos = 0;
                            isUpper = false;
                        }
                    }
                    else
                    {
                        switch (pos)
                        {
                            case 0:
                            {
                                deltaFraction.B0.NumberPart = val.Value;
                                break;
                            }
                            case 1:
                            {
                                deltaFraction.B1.NumberPart = val.Value;
                                break;
                            }
                            default:
                            {
                                deltaFraction.B2.NumberPart = val.Value;
                                break;
                            }
                        }

                        if (pos < 2)
                            pos++;
                    }

                    break;
                }
                case { Key: ConsoleKey.Q }:
                case { Key: ConsoleKey.Escape }:
                {
                    return (null, top.Value);
                }
                case { Key: ConsoleKey.Enter }:
                {
                    if ((deltaFraction.T0.IsZero() &&
                         deltaFraction.T1.IsZero() &&
                         deltaFraction.T2.IsZero()) ||
                        (deltaFraction.B0.IsZero() &&
                         deltaFraction.B1.IsZero() &&
                         deltaFraction.B2.IsZero()))
                        break;

                    return (deltaFraction, top.Value);
                }
                default:
                    continue;
            }

            RenderDeltaFraction(deltaFraction, isUpper, pos, top);
        }
    }

    private static void RenderDeltaFraction(DeltaFraction deltaFraction, bool isUpper, int pos,
        int? top = null)
    {
        StringBuilder buffer1 = new();
        StringBuilder buffer2 = new();

        if (isUpper)
        {
            RenderSimpleVariableToBuffer(deltaFraction.T0, pos is 0, buffer1);
            buffer1.Append("[Cyan]x^2[/Cyan] + ");
            RenderSimpleVariableToBuffer(deltaFraction.T1, pos is 1, buffer1);
            buffer1.Append("[Cyan]x[/Cyan] + ");
            RenderSimpleVariableToBuffer(deltaFraction.T2, pos is 2, buffer1);
            RenderSimpleVariableToBuffer(deltaFraction.B0, buffer2);
            buffer2.Append("[Cyan]x^2[/Cyan] + ");
            RenderSimpleVariableToBuffer(deltaFraction.B1, buffer2);
            buffer2.Append("[Cyan]x[/Cyan] + ");
            RenderSimpleVariableToBuffer(deltaFraction.B2, buffer2);
        }
        else
        {
            RenderSimpleVariableToBuffer(deltaFraction.T0, buffer1);
            buffer1.Append("[Cyan]x^2[/Cyan] + ");
            RenderSimpleVariableToBuffer(deltaFraction.T1, buffer1);
            buffer1.Append("[Cyan]x[/Cyan] + ");
            RenderSimpleVariableToBuffer(deltaFraction.T2, buffer1);
            RenderSimpleVariableToBuffer(deltaFraction.B0, pos is 0, buffer2);
            buffer2.Append("[Cyan]x^2[/Cyan] + ");
            RenderSimpleVariableToBuffer(deltaFraction.B1, pos is 1, buffer2);
            buffer2.Append("[Cyan]x[/Cyan] + ");
            RenderSimpleVariableToBuffer(deltaFraction.B2, pos is 2, buffer2);
        }

        var buffer1RealLength = buffer1.Length - 26 - (isUpper ? 15 : 0);
        var buffer2RealLength = buffer2.Length - 26 - (isUpper ? 0 : 15);

        int dashCharsCount;
        if (buffer1RealLength >= buffer2RealLength)
        {
            dashCharsCount = buffer1RealLength + 2;

            if (buffer1RealLength != buffer2RealLength)
            {
                var offset = (buffer1RealLength - buffer2RealLength) / 2;
                buffer2.Insert(0, new string(' ', offset));
                buffer2RealLength += offset;
            }
        }
        else
        {
            dashCharsCount = buffer2RealLength + 2;

            var offset = (buffer2RealLength - buffer1RealLength) / 2;
            buffer1.Insert(0, new string(' ', offset));
            buffer1RealLength += offset;
        }

        top ??= Console.CursorTop;
        var currTop = Console.CursorTop;
        ConsoleHelpers.SafeSetCursorPosition(0, top.Value);

        buffer1.Insert(0, "      ");
        buffer1.Append(new string(' ', Math.Clamp(Console.WindowWidth - buffer1RealLength - 7, 0, int.MaxValue)));

        buffer1.Append("\n A = ");
        buffer1.Append(new string('-', dashCharsCount));
        buffer1.Append(new string(' ', Math.Clamp(Console.WindowWidth - dashCharsCount - 6, 0, int.MaxValue)));

        buffer1.Append("\n      ");
        buffer1.Append(buffer2);
        buffer1.Append(new string(' ', Math.Clamp(Console.WindowWidth - buffer2RealLength - 7, 0, int.MaxValue)));

        ConsoleHelpers.WriteEmbeddedColorLine(buffer1.ToString());
        ConsoleHelpers.SafeSetCursorPosition(0, currTop);
    }

    public static void RenderFinalDelta(FinalDelta finalDelta)
    {
        var strBuilder = new StringBuilder();

        RenderSimpleVariableToBuffer(finalDelta.T0, strBuilder);
        strBuilder.Append("[Cyan]A^2[/Cyan] + ");
        RenderSimpleVariableToBuffer(finalDelta.T1, strBuilder);
        strBuilder.Append("[Cyan]A[/Cyan] + ");
        RenderSimpleVariableToBuffer(finalDelta.T2, strBuilder);

        ConsoleHelpers.WriteEmbeddedColorLine(strBuilder.ToString());
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
            buffer.Append("[/Green]");

        // if (simpleVariable.Power is 0)
        //     return;
        //
        // buffer.Append("[Cyan]x");
        // if (simpleVariable.Power is 1)
        // {
        //     buffer.Append("[/Cyan]");
        //     return;
        // }
        //
        // buffer.Append('^');
        // buffer.Append(simpleVariable.Power);
        // buffer.Append("[/Cyan]");
    }
}