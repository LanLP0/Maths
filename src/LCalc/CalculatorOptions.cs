namespace LCalc;

internal sealed class CalculatorOptions
{
    public bool StepByStep { get; set; }

    public bool Raw { get; set; }

    public bool ShowTree { get; set; }
    // public List<string> SolveFor { get; set; } = new();
}

[Flags]
internal enum CalculatorOption
{
    StepByStep = 1,
    Raw = 1 << 1,
    ShowTree = 1 << 2,
    Solve = 1 << 3
}