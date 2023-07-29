namespace LCalc.Variables;

internal sealed class Variable
{
    public Variable(string name, double value)
    {
        Name = name;
        Value = value;
    }

    public string Name { get; }
    public double Value { get; set; }
}