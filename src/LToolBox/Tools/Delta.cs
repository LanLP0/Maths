using Delta.Core.UI;

namespace LToolBox.Tools;

internal class Delta : Tool
{
    public override string ToolName { get; } = "delta";

    public override string? HelpMsg { get; } = "Calculate a fraction using delta\nType `q` to exit";

    public override void Execute() =>
        ConsoleEntry.Execute();
}