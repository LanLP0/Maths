namespace LCalc;

[Flags]
internal enum CalculatorOption
{
    StepByStep = 1,
    Raw = 1 << 1,
    ShowTree = 1 << 2,
    Solve = 1 << 3,
    VariableAllowed = 1 << 4,
    CompareAllowed = 1 << 5,
    CalculatorOptionAllowed = 1 << 6,
}