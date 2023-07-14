using System.Collections;

namespace LCalc;

internal sealed class VariableCollection : IVariableCollection
{
    private readonly List<Variable> _variables = new();
    private IVariableCollection? _linkedCollection;

    public int Count => _variables.Count;

    public IEnumerator<Variable> GetEnumerator()
    {
        return _variables.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool Contains(Variable variable)
    {
        if (ContainsName(variable.Name))
            return true;

        if (_linkedCollection is null)
            return false;

        return _linkedCollection.Contains(variable);
    }

    public bool Contains(string name)
    {
        if (ContainsName(name))
            return true;

        if (_linkedCollection is null)
            return false;

        return _linkedCollection.Contains(name);
    }

    public bool TryAdd(string name, double value)
    {
        return TryAdd(new Variable(name, value));
    }

    public bool TryAdd(Variable variable)
    {
        if (ContainsName(variable.Name)) // Dont use this.Contains() to allow for variable overwrite
            return false;

        _variables.Add(variable);
        return true;
    }

    public void OverrideAdd(string name, double value)
    {
        OverrideAdd(new Variable(name, value));
    }

    public void OverrideAdd(Variable variable)
    {
        foreach (var var1 in _variables)
        {
            if (var1.Name != variable.Name)
                continue;

            var1.Value = variable.Value;
            return;
        }

        _variables.Add(variable);
    }

    public bool TryGet(string name, out double result)
    {
        foreach (var variable in _variables)
        {
            if (variable.Name != name)
                continue;

            result = variable.Value;
            return true;
        }

        if (_linkedCollection is not null)
            return _linkedCollection.TryGet(name, out result);

        switch (name)
        {
            case "pi":
                result = Math.PI;
                return true;
            case "e":
                result = Math.E;
                return true;
            case "tau":
                result = 6.283185307179586476925; // tau
                return true;
            default:
                result = -1;
                return false;
        }
    }

    public int Remove(string name)
    {
        return _variables.RemoveAll(x => x.Name == name);
    }

    public void Link(IVariableCollection? variableCollection)
    {
        _linkedCollection = variableCollection;
    }

    private bool ContainsName(string name)
    {
        foreach (var variable in _variables)
            if (variable.Name == name)
                return true;

        return false;
    }
}