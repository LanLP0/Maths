using LToolBox.Delta.UI;

namespace LToolBox.Tools;

internal class DeltaTool : Tool
{
    public override string ToolName { get; } = "delta";

    public override string? HelpMsg { get; } = "Calculate a fraction using delta\nType `q` to exit";

    public override void Execute() =>
        DeltaCli.Execute();
}