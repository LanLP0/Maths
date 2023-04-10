using Common.Cli;
using Common.Maths;
using Spectre.Console;

namespace LToolBox.Tools;

internal sealed class IsPrimeTool : Tool
{
    public IsPrimeTool(IAnsiConsole console) : base(console)
    {
    }

    public override string ToolName { get; } = "isprime";

    public override string? HelpMsg { get; } = "Check is a number is prime or not\nPress [Yellow]Esc[/] to exit";

    public override void Execute()
    {
        var value = Console.Ask<int?>("[white]Number:[/]", true, clear: false, newLine: false);
        if (value is null)
            return;

        Console.Markup("[white]Result:[/] ");
        Console.WriteLine(Maths.IsPrime(value.Value).ToString());
    }
}