namespace LCalc;

[Flags]
public enum CalculatorOption
{
    None = 0,
    Step = 1,
    Tree = 1 << 1,
    Solve = 1 << 2,
    VariableAllowed = 1 << 3,
    CompareAllowed = 1 << 4,
    CalculatorOptionAllowed = 1 << 5,
    LaTeX = 1 << 6,
    LaTeXDoc = 1 << 7
}