namespace LToolBox;

public abstract class Tool
{
    public virtual string? HelpMsg { get; } = null;

    public abstract string ToolName { get; }

    public abstract void Execute();

    public virtual void ExecuteHelp()
    {
        Console.WriteLine(HelpMsg ?? "Sorry there is no help for this tool");
    }
}