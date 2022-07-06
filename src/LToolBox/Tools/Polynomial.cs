using Common;

namespace LToolBox.Tools;

internal class Polynomial : Tool
{
    public override string ToolName { get; } = "polynomial";

    public override string? HelpMsg { get; } = "Calculate polynomial\nType `q` to quit";

    public override void Execute()
    {
        double a = 0, b = 0, c = 0;
        var pos = 0;
        var row = Console.CursorTop;
        ReRenderExp(a, b, c, pos);

        for (;;)
        {
            var input = Console.ReadKey(true);

            switch (input.Key)
            {
                case ConsoleKey.LeftArrow:
                case ConsoleKey.H:
                {
                    if (pos is 0)
                        continue;

                    pos--;
                    break;
                }
                case ConsoleKey.RightArrow:
                case ConsoleKey.L:
                {
                    if (pos >= 2)
                        continue;

                    pos++;
                    break;
                }
                case ConsoleKey.Spacebar:
                {
                    var val = ConsoleHelpers.PromptIntAndClearLine("Value: ", row + 1);
                    Console.CursorTop = row;

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
                    
                    break;
                }
                case ConsoleKey.Q:
                case ConsoleKey.Escape:
                {
                    Console.WriteLine();
                    return;
                }
                case ConsoleKey.Enter:
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
        switch (Common.Maths.Polynomial.Calc2(a, b, c, out var result1, out var result2))
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