namespace LCalc;

[Flags]
public enum CalculatorOption
{
    None = 0,
    Step = 1,
    Raw = 1 << 1,
    Tree = 1 << 2,
    Solve = 1 << 3,
    VariableAllowed = 1 << 4,
    CompareAllowed = 1 << 5,
    CalculatorOptionAllowed = 1 << 6,
    LaTeX = 1 << 7,
    LaTeXDoc = 1 << 8
}