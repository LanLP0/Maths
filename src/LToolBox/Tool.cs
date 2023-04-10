using Spectre.Console;

namespace LToolBox;

internal abstract class Tool
{
    public Tool(IAnsiConsole console)
    {
        Console = console;
    }

    public IAnsiConsole Console { get; }

    public virtual string? HelpMsg { get; } = null;

    public abstract string ToolName { get; }

    public abstract void Execute();

    public virtual void ExecuteHelp()
    {
        Console.MarkupLine(HelpMsg ?? "Sorry there is no help for this tool");
    }
}