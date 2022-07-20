using System.Text;
using Common.Cli;
using Delta.Core;

namespace LToolBox.Delta.UI;

internal static class DeltaHelpers
{
    public static DeltaFraction? PromptDelta(int? row = null)
    {
        row ??= Console.CursorTop;
        
        Console.SetCursorPosition(0, row.Value + 3);
        
        var isUpper = true;
        int pos = 0;
        var deltaFraction = new DeltaFraction
        {
            T0 = { NumberPart = 0, Power = 2, PowerOfA = 0},
            T1 = { NumberPart = 0, Power = 1, PowerOfA = 0},
            T2 = { NumberPart = 0, Power = 0, PowerOfA = 0},
            B0 = { NumberPart = 0, Power = 2 },
            B1 = { NumberPart = 0, Power = 1 },
            B2 = { NumberPart = 0, Power = 0 }
        };
        
        RenderDeltaFraction(deltaFraction, (isUpper, pos), row);

        for (;;)
        {
            var input = Console.ReadKey(true);

            switch (input.Key)
            {
                case ConsoleKey.DownArrow:
                case ConsoleKey.J:
                {
                    if (!isUpper)
                        break;

                    isUpper = false;
                    break;
                }
                case ConsoleKey.UpArrow:
                case ConsoleKey.K:
                {
                    if (isUpper)
                        break;

                    isUpper = true;
                    break;
                }
                case ConsoleKey.LeftArrow:
                case ConsoleKey.H:
                {
                    if (pos is 0)
                        break;

                    pos--;
                    break;
                }
                case ConsoleKey.RightArrow:
                case ConsoleKey.L:
                {
                    if (pos >= 2)
                        break;

                    pos++;
                    break;
                }
                case ConsoleKey.D0:
                case ConsoleKey.D1:
                case ConsoleKey.D2:
                case ConsoleKey.D3:
                case ConsoleKey.D4:
                case ConsoleKey.D5:
                case ConsoleKey.D6:
                case ConsoleKey.D7:
                case ConsoleKey.D8:
                case ConsoleKey.D9:
                case ConsoleKey.NumPad0:
                case ConsoleKey.NumPad1:
                case ConsoleKey.NumPad2:
                case ConsoleKey.NumPad3:
                case ConsoleKey.NumPad4:
                case ConsoleKey.NumPad5:
                case ConsoleKey.NumPad6:
                case ConsoleKey.NumPad7:
                case ConsoleKey.NumPad8:
                case ConsoleKey.NumPad9:
                {
                    var val = ConsoleHelpers.PromptIntAndClearLine("Value: ", row.Value + 3, defaultValue: EnumHelpers.FastConsoleKeyToNumberString(input.Key));

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
                    }

                    break;
                }
                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                {
                    return null;
                }
                case ConsoleKey.Enter:
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
            
            RenderDeltaFraction(deltaFraction, (isUpper, pos), row);
        }
    }

    public static void RenderDeltaFraction(DeltaFraction deltaFraction, (bool isUpper, int pos) selected, int? row = null)
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

        row ??= Console.CursorTop;
        var currTop = Console.CursorTop;
        Console.SetCursorPosition(0, row.Value);

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

    public static void RenderFinalDelta(FinalDelta finalDelta, int? row = null)
    {
        var strBuilder = new StringBuilder();
        
        row ??= Console.CursorTop;
        var currTop = Console.CursorTop;
        var currLeft = Console.CursorLeft;
        Console.SetCursorPosition(0, row.Value);

        RenderSimpleVariableToBuffer(finalDelta.T0, strBuilder);
        strBuilder.Append("[Cyan]A^2[/Cyan] + ");
        RenderSimpleVariableToBuffer(finalDelta.T1, strBuilder);
        strBuilder.Append("[Cyan]A[/Cyan] + ");
        RenderSimpleVariableToBuffer(finalDelta.T2, strBuilder);
        
        ConsoleHelpers.WriteEmbeddedColorLine(strBuilder.ToString());
        Console.SetCursorPosition(currLeft, currTop);
    }

    private static void RenderSimpleVariableToBuffer(SimpleVariable simpleVariable, StringBuilder buffer) =>
        RenderSimpleVariableToBuffer(simpleVariable, false, buffer);

    private static void RenderSimpleVariableToBuffer(SimpleVariable simpleVariable, bool isSelected, StringBuilder buffer)
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