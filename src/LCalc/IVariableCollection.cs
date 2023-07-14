namespace LCalc;

internal interface IVariableCollection : IEnumerable<Variable>
{
    int Count { get; }

    bool Contains(Variable variable);
    bool Contains(string name);
    bool TryAdd(string name, double value);
    bool TryAdd(Variable variable);
    void OverrideAdd(string name, double value);
    void OverrideAdd(Variable variable);
    bool TryGet(string name, out double result);
    int Remove(string name);
    void Link(IVariableCollection? variableCollection);
}