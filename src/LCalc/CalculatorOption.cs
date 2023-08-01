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
    LaTeXDoc = 1 << 7,
    /// <summary>
    ///     Allows latex but not latexdoc, useful for situation<br />
    ///     when the full doc is not needed and should not be used
    /// </summary>
    NoLaTeXDoc = 1 << 8
}