using Common.Cli;
using Common.Maths.Expressions;
using Spectre.Console;
#if DEBUG
using Serilog;
#endif

namespace LToolBox.Tools;

internal sealed class ComplexCalcTool : Tool
{
    public override string ToolName { get; } = "complexcalc";

    public override string? HelpMsg { get; } = """
        Calculate multiple expression of any size
        Enter constant by start typing it
        [Yellow]Arrow keys[/], [Yellow][[[/] - [Yellow]]][/]: Move around
        [Yellow]Ctrl-+[/], [Yellow]+[/]: decrease, increase amount of element
        [Yellow]a[/] - [Yellow]z[/]: Change variable power
        Press [Yellow]\[/] to exit
        """;

    private readonly Validator<int?> _powValidator = new()
    {
        val =>
        {
            if (val!.Value > 10) return "The value must be <= 10";

            return null;
        },
        val =>
        {
            if (val!.Value <= 0) return "The value must be positive";

            return null;
        }
    };

    public override void Execute()
    {
        var ex = PromptExpression();
        if (ex is null)
            return;

        var firstPrompt = true;
        for (;;)
        {
            var op = Console.ChooseOption("[blue][[Optional]][/]Op [blue](*/^/+/-)[/]:", new[] { "*", "^", "+", "-" },
                true, newLine: false, clear: false);

            if (op is null)
            {
                if (!firstPrompt)
                    return;

                Console.WriteLine("Result:");

                ex.Sort();

                RenderExpression(ex, -1);
                Console.WriteLine();

                return;
            }

            firstPrompt = false;

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
                    var pow = Console.Ask("Power by:", validator: _powValidator, newLine: false, clear: false);
                    if (!pow.HasValue)
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

            ex.Sort();

            Console.WriteLine("Result:");
            RenderExpression(ex, -1);

            Console.WriteLine();
        }
    }

    private Expression? PromptExpression()
    {
        var result = new Expression
        {
            Values =
            {
                new Element()
            }
        }; // Initial empty Expression

        const int maxValCount = 7; // Max amount of vals an Expression can have
        var selectedPos = 0;

        RenderExpression(result, selectedPos);

        for (;;) // Main loop
        {
            var input = Console.ReadKey(true)!.Value;

            switch (input)
            {
                case { KeyChar: '=' }: // Add val
                {
                    // If the number of vals has reached the max amount
                    if (result.Values.Count >= maxValCount)
                        continue;

                    result.Values.Add(new Element());
                    selectedPos = result.Values.Count - 1; // Set the selected val to the newly added val
                    break;
                }
                case { KeyChar: '+' }: // Remove val
                {
                    // The should always be some val(s)
                    if (result.Values.Count <= 1)
                        continue;

                    // Remove the right-most one
                    result.Values.RemoveAt(result.Values.Count - 1);

                    selectedPos = Math.Clamp(selectedPos, 0, result.Values.Count - 1);
                    break;
                }
                case { Key: ConsoleKey.LeftArrow }:
                case { KeyChar: '[' }: // Move left
                {
                    // If the first one is selected, ignore
                    if (selectedPos is 0)
                        break;

                    selectedPos--;
                    break;
                }
                case { Key: ConsoleKey.RightArrow }:
                case { KeyChar: ']' }: // Move right
                {
                    // If the last one is selected, ignore
                    if (selectedPos + 1 >= result.Values.Count)
                        break;

                    selectedPos++;
                    break;
                }
                case { Key: ConsoleKey.Escape }:
                case { KeyChar: '\\' }: // Exit
                {
                    Console.WriteLine();
                    return null;
                }
                case { Key: ConsoleKey.A }:
                case { Key: ConsoleKey.B }:
                case { Key: ConsoleKey.C }:
                case { Key: ConsoleKey.D }:
                case { Key: ConsoleKey.E }:
                case { Key: ConsoleKey.F }:
                case { Key: ConsoleKey.G }:
                case { Key: ConsoleKey.H }:
                case { Key: ConsoleKey.I }:
                case { Key: ConsoleKey.J }:
                case { Key: ConsoleKey.K }:
                case { Key: ConsoleKey.L }:
                case { Key: ConsoleKey.M }:
                case { Key: ConsoleKey.N }:
                case { Key: ConsoleKey.O }:
                case { Key: ConsoleKey.P }:
                case { Key: ConsoleKey.Q }:
                case { Key: ConsoleKey.R }:
                case { Key: ConsoleKey.S }:
                case { Key: ConsoleKey.T }:
                case { Key: ConsoleKey.U }:
                case { Key: ConsoleKey.V }:
                case { Key: ConsoleKey.W }:
                case { Key: ConsoleKey.X }:
                case { Key: ConsoleKey.Y }:
                case { Key: ConsoleKey.Z }: // Set the unknowns
                {
                    var e = result.Values[selectedPos]; // The current selected val
                    if (e.Unknowns.Count > 6)
                        break;

                    var key = char.ToLower(input.KeyChar) - 97; // Get the unknown's id
                    if (e.Unknowns.Remove(key))
                        break;

                    var val = Console.Ask<int?>($"{input.KeyChar}:", true);

                    if (val is null or 0)
                        break;

                    e.Unknowns.Add(key, val.Value);
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
                case { Key: ConsoleKey.NumPad9 }: // Change value of selected
                {
                    var val = Console.Ask<int?>("Value:", initialText: EnumHelpers.FastToString(input.Key));

                    if (!val.HasValue)
                        continue;

                    result.Values[selectedPos].Value = val.Value;
                    break;
                }
                case { KeyChar: '-' }: // Change value of selected
                {
                    var val = Console.Ask<int?>("Value:", initialText: "-");

                    if (!val.HasValue)
                        continue;

                    result.Values[selectedPos].Value = val.Value;

                    break;
                }
                case { Key: ConsoleKey.Enter }:
                {
                    if (!result.Validate())
                        continue;

                    Console.WriteLine();
                    result = result.Condense();
                    return result;
                }
                default:
                    continue;
            }

            RenderExpression(result, selectedPos);
        }
    }

    private void RenderExpression(Expression expression, int selectedPos)
    {
        Console.ClearLine();
        Console.Markup(expression.ToMarkupColorString(selectedPos));
        Console.MoveCursorToStart();
    }
    
    public ComplexCalcTool(IAnsiConsole console) : base(console)
    {
    }
}