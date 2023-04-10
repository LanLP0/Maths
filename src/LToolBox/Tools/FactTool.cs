using Common.Cli;
using Common.Maths;
using Spectre.Console;

namespace LToolBox.Tools;

internal sealed class FactTool : Tool
{
    public FactTool(IAnsiConsole console) : base(console)
    {
    }

    public override string ToolName { get; } = "fact";

    public override string? HelpMsg { get; } = "Find fact(s) of a number\nPress [Yellow]Esc[/] to exit";

    public override void Execute()
    {
        var value = Console.Ask<int?>("[white]Number:[/]", true, clear: false, newLine: false);
        if (value is null)
            return;

        Console.Markup("[white]Result:[/] ");
        Console.WriteLine(string.Join(" * ", Maths.GetFact(value.Value)));
    }
}