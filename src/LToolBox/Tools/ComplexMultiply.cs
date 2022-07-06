using Common;
using Common.Maths.Expressions;
using Microsoft.Extensions.Logging;

namespace LToolBox.Tools;

public class ComplexMultiply : Tool
{
    private ILogger<ComplexMultiply> _logger;

    public ComplexMultiply(ILogger<ComplexMultiply> logger)
    {
        _logger = logger;
    }

    public override string ToolName { get; } = "complexmultiply";

    public override string? HelpMsg { get; } = "Multiply multiple expression of any size\nControls:\n`-`, `+`: decrease, increase amount of element\n<Shift+Minus>, <Shift+Plus>: decrease, increase amount of variable\n<Alt+Space>, `v`: change power value\nPress `q` to quit";

    public override void Execute()
    {
        var ex = PromptExpression();
        if (ex is null)
            return;
        
        for (;;)
        {
            Console.WriteLine("Multiply with:");
            var ex1 = PromptExpression();
            if (ex1 is null)
                return;

            ex *= ex1;
            Console.WriteLine("Result:");
            RenderExpression(ex, -1);
            
            Console.WriteLine();
        }
    }

    private Expression? PromptExpression()
    {
        var result = new Expression
        {
            Elements =
            {
                new Element()
            }
        };

        const int argLimit = 7;
        const int powerLimit = 5;
        var pos = 0;
        var (currLeft, currTop) = Console.GetCursorPosition();
        RenderExpression(result, pos);

        for (;;)
        {
            var input = Console.ReadKey(true);

            switch (input)
            {
                case {KeyChar: '='}:
                {
                    if (result.Elements.Count >= argLimit)
                        continue;

                    result.Elements.Add(new Element());
                    break;
                }
                case {KeyChar: '_'}:
                {
                    var powers = result.Elements[pos].Powers;
                    if (powers.Count <= 0)
                        continue;

                    powers.RemoveAt(powers.Count - 1);
                    break;
                }
                case {KeyChar: '+'}:
                {
                    var powers = result.Elements[pos].Powers;
                    if (powers.Count > powerLimit)
                        continue;
                        
                    powers.Add(1);
                    break;
                }
                case {KeyChar: '-'}:
                {
                    if (result.Elements.Count <= 1)
                        continue;
                    
                    result.Elements.RemoveAt(result.Elements.Count - 1);

                    pos = Math.Clamp(pos, 0, result.Elements.Count - 1);
                    break;
                }
                case {Key: ConsoleKey.LeftArrow}:
                case {KeyChar: 'h'}:
                {
                    if (pos is 0)
                        break;

                    pos--;
                    break;
                }
                case {Key: ConsoleKey.RightArrow}:
                case {KeyChar: 'l'}:
                {
                    if (pos + 1 >= result.Elements.Count)
                        break;

                    pos++;
                    break;
                }
                case {Key: ConsoleKey.Escape}:
                case {KeyChar: 'q'}:
                {
                    Console.WriteLine();
                    return null;
                }
                case { KeyChar: 'v' }:
                {
                    PromptChangePower(result.Elements[pos]);
                    break;
                }
                case {KeyChar: ' '}:
                {
                    if (input.Modifiers.HasFlag(ConsoleModifiers.Alt))
                    {
                        PromptChangePower(result.Elements[pos]);
                        break;
                    }
                    
                    var val = Common.ConsoleHelpers.PromptIntAndClearLine("Value: ", Console.CursorTop + 1);
                    
                    if (val is null)
                    {
                        Console.SetCursorPosition(currLeft, currTop);
                        continue;
                    }

                    result.Elements[pos].Value = val.Value;
                    break;
                }
                case {Key: ConsoleKey.Enter}:
                {
                    if (!result.Elements.TrueForAll(a => a.Value is not 0))
                        continue;
                    
                    Console.WriteLine();
#if DEBUG
                    _logger.LogDebug("{@resultEx}", result);
#endif
                    return result.Collapse();
                }
                default:
                    continue;
            }
            
            Console.SetCursorPosition(currLeft, currTop);
            RenderExpression(result, pos);
        }
    }

    private void RenderExpression(Expression expression, int selectedPos)
    {
        Common.ConsoleHelpers.WriteEmbeddedColor(expression.ToStringWithColor(selectedPos));

        var (currLeft, currTop) = Console.GetCursorPosition();
        
        Console.Write(new string(' ', Math.Clamp(Console.WindowWidth - Console.CursorLeft - 1, 0, int.MaxValue)));
        
        Console.SetCursorPosition(currLeft, currTop);
    }

    private void PromptChangePower(Element element)
    {
        var (currLeft, currTop) = Console.GetCursorPosition();

        Console.SetCursorPosition(0, currTop + 1);
        for (var i = 0; i < element.Powers.Count; i++)
        {
            var val = ConsoleHelpers.PromptIntAndClearLine((char)(i % 26 + 97) + ": ",
                defaultValue: element.Powers[i].ToString());
            
            if (val is null)
                continue;

            element.Powers[i] = val.Value;
        }
    }
}