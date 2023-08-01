using System.Collections;

namespace LCalc.Variables;

/// <summary>
///     Used to efficiently override a single variable
/// </summary>
internal sealed class SingleVariableCollection : IVariableCollection
{
    private IVariableCollection? _linkedCollection;

    public SingleVariableCollection(Variable variable, IVariableCollection? linkedCollection)
    {
        Variable = variable;
        _linkedCollection = linkedCollection;
    }

    public Variable Variable { get; }

    public IEnumerator<Variable> GetEnumerator()
    {
        yield return Variable;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count => 1;

    public bool Contains(Variable variable)
    {
        return Contains(variable.Name);
    }

    public bool Contains(string name)
    {
        if (name == Variable.Name)
            return false;

        return _linkedCollection?.Contains(name) ?? false;
    }

    public bool TryAdd(string name, double value)
    {
        throw new NotImplementedException();
    }

    public bool TryAdd(Variable variable)
    {
        throw new NotImplementedException();
    }

    public void OverrideAdd(string name, double value)
    {
        throw new NotImplementedException();
    }

    public void OverrideAdd(Variable variable)
    {
        throw new NotImplementedException();
    }

    public bool TryGet(string name, out double result)
    {
        if (Variable.Name == name)
        {
            result = Variable.Value;
            return true;
        }

        if (_linkedCollection is not null && _linkedCollection.TryGet(name, out result))
            return true;

        result = double.NaN;
        return false;
    }

    public int Remove(string name)
    {
        throw new NotImplementedException();
    }

    public void Link(IVariableCollection? variableCollection)
    {
        _linkedCollection = variableCollection;
    }
}