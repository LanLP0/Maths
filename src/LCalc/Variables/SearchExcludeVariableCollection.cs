using System.Collections;

namespace LCalc.Variables;

/// <summary>
/// Used to search for a variable in a collection but exclude one variable name
/// </summary>
internal sealed class SearchExcludeVariableCollection : IVariableCollection
{
    private IVariableCollection _variableCollection;
    /// <summary>
    /// The name to exclude
    /// </summary>
    private string _name;

    public SearchExcludeVariableCollection(string name, IVariableCollection variableCollection)
    {
        _name = name;
        _variableCollection = variableCollection;
    }

    public IEnumerator<Variable> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public int Count { get; }
    public bool Contains(Variable variable)
    {
        return Contains(variable.Name);
    }

    public bool Contains(string name)
    {
        if (name == _name)
            return false;

        return _variableCollection.Contains(name);
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
        throw new NotImplementedException();
    }

    public int Remove(string name)
    {
        throw new NotImplementedException();
    }

    public void Link(IVariableCollection? variableCollection)
    {
        throw new NotImplementedException();
    }
}