using Common.Cli;
using Common.Cli.Maths.Expressions;
using Microsoft.Extensions.Logging;

namespace LToolBox.Tools;

internal class ComplexMultiplyTool : Tool
{
#if DEBUG
    private ILogger<ComplexMultiplyTool> _logger;

    public ComplexMultiplyTool(ILogger<ComplexMultiplyTool> logger)
    {
        _logger = logger;
    }
#endif

    public override string ToolName { get; } = "complexcalc";

    public override string? HelpMsg { get; } = "Calculate multiple expression of any size\nControls:\nArrow keys, `[` - `]`: Move around\n`1` - `9`: Change variable value\n<Ctrl-Minus>, `+`: decrease, increase amount of element\n`a` - `z`: Add/Remove power\nPress `\\` to quit";

    public override void Execute()
    {
        var ex = PromptExpression();
        if (ex is null)
            return;
        
        for (;;)
        {
            var op = ConsoleHelpers.ChooseOption("Op (*/^/+/-): ", new[] {"*", "^", "+", "-"}, Console.CursorTop + 1, required: false)!;

            if (op is null)
                return;

            switch (op.First())
            {
                case '*':
                {
                    Console.WriteLine("Multiply by:");
                    var ex1 = PromptExpression();
                    if (ex1 is null)
                        return;

                    ex *= ex1;
                    break;
                }
                case '^':
                {
                    var pow = ConsoleHelpers.PromptInt("Power by: ", lengthLimit: 2, isNegativeAllowed: false);
                    if (pow is null)
                        return;

                    ex = Expression.Pow(ex, pow.Value);
                    break;
                }
                case '-':
                {
                    Console.WriteLine("Minus by:");
                    var ex1 = PromptExpression();
                    if (ex1 is null)
                        return;

                    ex -= ex1;
                    break;
                }
                case '+':
                {
                    Console.WriteLine("Plus by:");
                    var ex1 = PromptExpression();
                    if (ex1 is null)
                        return;

                    ex += ex1;
                    break;
                }
            }
            Console.WriteLine("Result:");
#if DEBUG
            _logger.LogDebug("{@ex}", ex);
#endif
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
                    pos = result.Elements.Count - 1;
                    break;
                }
                // case {KeyChar: '_'}:
                // {
                //     var powers = result.Elements[pos].Powers;
                //     if (powers.Count <= 0)
                //         continue;

                //     powers.RemoveAt(powers.Count - 1);
                //     break;
                // }
                // case {KeyChar: '+'}:
                // {
                //     var powers = result.Elements[pos].Powers;
                //     if (powers.Count > powerLimit)
                //         continue;
                        
                //     powers.Add(1);
                //     break;
                // }
                case {KeyChar: '+'}:
                {
                    if (result.Elements.Count <= 1)
                        continue;
                    
                    result.Elements.RemoveAt(result.Elements.Count - 1);

                    pos = Math.Clamp(pos, 0, result.Elements.Count - 1);
                    break;
                }
                case {Key: ConsoleKey.LeftArrow}:
                case {KeyChar: '['}:
                {
                    if (pos is 0)
                        break;

                    pos--;
                    break;
                }
                case {Key: ConsoleKey.RightArrow}:
                case {KeyChar: ']'}:
                {
                    if (pos + 1 >= result.Elements.Count)
                        break;

                    pos++;
                    break;
                }
                case {Key: ConsoleKey.Escape}:
                case {KeyChar: '\\'}:
                {
                    Console.WriteLine();
                    return null;
                }
                case {Key: ConsoleKey.A}:
                case {Key: ConsoleKey.B}:
                case {Key: ConsoleKey.C}:
                case {Key: ConsoleKey.D}:
                case {Key: ConsoleKey.E}:
                case {Key: ConsoleKey.F}:
                case {Key: ConsoleKey.G}:
                case {Key: ConsoleKey.H}:
                case {Key: ConsoleKey.I}:
                case {Key: ConsoleKey.J}:
                case {Key: ConsoleKey.K}:
                case {Key: ConsoleKey.L}:
                case {Key: ConsoleKey.M}:
                case {Key: ConsoleKey.N}:
                case {Key: ConsoleKey.O}:
                case {Key: ConsoleKey.P}:
                case {Key: ConsoleKey.Q}:
                case {Key: ConsoleKey.R}:
                case {Key: ConsoleKey.S}:
                case {Key: ConsoleKey.T}:
                case {Key: ConsoleKey.U}:
                case {Key: ConsoleKey.V}:
                case {Key: ConsoleKey.W}:
                case {Key: ConsoleKey.X}:
                case {Key: ConsoleKey.Y}:
                case {Key: ConsoleKey.Z}:
                {
                    var e = result.Elements[pos];
                    if (e.Powers.Count > 6)
                        break;

                    var key = (int)char.ToLower(input.KeyChar) - 97;
                    if (e.Powers.ContainsKey(key))
                    {
                        e.Powers.Remove(key);
                        break;
                    }

                    var val = ConsoleHelpers.PromptIntAndClearLine(input.KeyChar + ": ", Console.CursorTop + 1);

                    if (!val.HasValue)
                        break;

                    if (val.Value is 0)
                        break;

                    e.Powers.Add(key, val.Value);
                    break;
                }
                case {Key: ConsoleKey.D0}:
                case {Key: ConsoleKey.D1}:
                case {Key: ConsoleKey.D2}:
                case {Key: ConsoleKey.D3}:
                case {Key: ConsoleKey.D4}:
                case {Key: ConsoleKey.D5}:
                case {Key: ConsoleKey.D6}:
                case {Key: ConsoleKey.D7}:
                case {Key: ConsoleKey.D8}:
                case {Key: ConsoleKey.D9}:
                case {Key: ConsoleKey.NumPad0}:
                case {Key: ConsoleKey.NumPad1}:
                case {Key: ConsoleKey.NumPad2}:
                case {Key: ConsoleKey.NumPad3}:
                case {Key: ConsoleKey.NumPad4}:
                case {Key: ConsoleKey.NumPad5}:
                case {Key: ConsoleKey.NumPad6}:
                case {Key: ConsoleKey.NumPad7}:
                case {Key: ConsoleKey.NumPad8}:
                case {Key: ConsoleKey.NumPad9}:
                {
                    var val = Common.Cli.ConsoleHelpers.PromptIntAndClearLine("Value: ", Console.CursorTop + 1, defaultValue: EnumHelpers.FastConsoleKeyToNumberString(input.Key));
                    
                    if (val is null)
                    {
                        Console.SetCursorPosition(currLeft, currTop);
                        continue;
                    }

                    result.Elements[pos].Value = val.Value;
                    break;
                }
                case {KeyChar: '-'}:
                {
                    var val = Common.Cli.ConsoleHelpers.PromptIntAndClearLine("Value: ", Console.CursorTop + 1, defaultValue: "-");
                    
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
                    result = result.Collapse();
#if DEBUG
                    _logger.LogDebug("{@resultEx}", result);
#endif
                    return result;
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
        Common.Cli.ConsoleHelpers.WriteEmbeddedColor(expression.ToStringWithColor(selectedPos));

        var (currLeft, currTop) = Console.GetCursorPosition();
        
        Console.Write(new string(' ', Math.Clamp(Console.WindowWidth - Console.CursorLeft - 1, 0, int.MaxValue)));
        
        Console.SetCursorPosition(currLeft, currTop);
    }
}