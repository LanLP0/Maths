using System.Text;
using Common.Cli;
using Delta.Core;

namespace LToolBox.Delta;

internal static class DeltaHelpers
{
    public static DeltaFraction? PromptDelta(int? top = null)
    {
        top ??= Console.CursorTop;

        Console.SetCursorPosition(0, top.Value + 3);

        var isUpper = true;
        var pos = 0;
        var deltaFraction = new DeltaFraction
        {
            T0 = { NumberPart = 0, Power = 2, PowerOfA = 0 },
            T1 = { NumberPart = 0, Power = 1, PowerOfA = 0 },
            T2 = { NumberPart = 0, Power = 0, PowerOfA = 0 },
            B0 = { NumberPart = 0, Power = 2 },
            B1 = { NumberPart = 0, Power = 1 },
            B2 = { NumberPart = 0, Power = 0 }
        };

        RenderDeltaFraction(deltaFraction, (isUpper, pos), top);

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
                    var val = ConsoleHelpers.PromptIntAndClearLine("Value: ", top.Value + 3,
                        defaultValue: EnumHelpers.FastConsoleKeyToNumberString(input.Key));

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
                    var val = ConsoleHelpers.PromptIntAndClearLine("Value: ", top.Value + 3, defaultValue: "-");

                    if (!val.HasValue)
                        break;

                    if (isUpper)
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
                    else
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

                    break;
                }
                case { Key: ConsoleKey.Q }:
                case { Key: ConsoleKey.Escape }:
                {
                    return null;
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

                    return deltaFraction;
                }
                default:
                    continue;
            }

            RenderDeltaFraction(deltaFraction, (isUpper, pos), top);
        }
    }

    private static void RenderDeltaFraction(DeltaFraction deltaFraction, (bool isUpper, int pos) selected,
        int? top = null)
    {
        StringBuilder buffer1 = new();
        StringBuilder buffer2 = new();

        if (selected.isUpper)
        {
            RenderSimpleVariableToBuffer(deltaFraction.T0, selected.pos is 0, buffer1);
            buffer1.Append(" + ");
            RenderSimpleVariableToBuffer(deltaFraction.T1, selected.pos is 1, buffer1);
            buffer1.Append(" + ");
            RenderSimpleVariableToBuffer(deltaFraction.T2, selected.pos is 2, buffer1);
            RenderSimpleVariableToBuffer(deltaFraction.B0, buffer2);
            buffer2.Append(" + ");
            RenderSimpleVariableToBuffer(deltaFraction.B1, buffer2);
            buffer2.Append(" + ");
            RenderSimpleVariableToBuffer(deltaFraction.B2, buffer2);
        }
        else
        {
            RenderSimpleVariableToBuffer(deltaFraction.T0, buffer1);
            buffer1.Append(" + ");
            RenderSimpleVariableToBuffer(deltaFraction.T1, buffer1);
            buffer1.Append(" + ");
            RenderSimpleVariableToBuffer(deltaFraction.T2, buffer1);
            RenderSimpleVariableToBuffer(deltaFraction.B0, selected.pos is 0, buffer2);
            buffer2.Append(" + ");
            RenderSimpleVariableToBuffer(deltaFraction.B1, selected.pos is 1, buffer2);
            buffer2.Append(" + ");
            RenderSimpleVariableToBuffer(deltaFraction.B2, selected.pos is 2, buffer2);
        }

        var buffer1RealLength = buffer1.Length - 26 - (selected.isUpper ? 15 : 0);
        var buffer2RealLength = buffer2.Length - 26 - (selected.isUpper ? 0 : 15);

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
        Console.SetCursorPosition(0, top.Value);

        buffer1.Insert(0, "      ");
        buffer1.Append(new string(' ', Math.Clamp(Console.WindowWidth - buffer1RealLength - 6, 0, int.MaxValue)));

        buffer1.Append("\n A = ");
        buffer1.Append(new string('-', dashCharsCount));
        buffer1.Append(new string(' ', Math.Clamp(Console.WindowWidth - dashCharsCount - 5, 0, int.MaxValue)));

        buffer1.Append("\n      ");
        buffer1.Append(buffer2);
        buffer1.Append(new string(' ', Math.Clamp(Console.WindowWidth - buffer2RealLength - 6, 0, int.MaxValue)));

        ConsoleHelpers.WriteEmbeddedColorLine(buffer1.ToString());
        Console.SetCursorPosition(0, currTop);
    }

    public static void RenderFinalDelta(FinalDelta finalDelta, int? top = null)
    {
        var strBuilder = new StringBuilder();

        top ??= Console.CursorTop;
        var currTop = Console.CursorTop;
        var currLeft = Console.CursorLeft;
        Console.SetCursorPosition(0, top.Value);

        RenderSimpleVariableToBuffer(finalDelta.T0, strBuilder);
        strBuilder.Append("[Cyan]A^2[/Cyan] + ");
        RenderSimpleVariableToBuffer(finalDelta.T1, strBuilder);
        strBuilder.Append("[Cyan]A[/Cyan] + ");
        RenderSimpleVariableToBuffer(finalDelta.T2, strBuilder);

        ConsoleHelpers.WriteEmbeddedColorLine(strBuilder.ToString());
        Console.SetCursorPosition(currLeft, currTop);
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

        if (simpleVariable.Power is 0)
            return;

        buffer.Append("[Cyan]x");
        if (simpleVariable.Power is 1)
        {
            buffer.Append("[/Cyan]");
            return;
        }

        buffer.Append('^');
        buffer.Append(simpleVariable.Power);
        buffer.Append("[/Cyan]");
    }
}