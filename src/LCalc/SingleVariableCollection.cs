using System.Collections;
using System.Diagnostics;

namespace LCalc;

// Used to override a single variable
internal sealed class SingleVariableCollection : IVariableCollection
{
    private readonly Variable _variable;
    private readonly IVariableCollection _linkedCollection;

    public Variable Variable
    {
        get => _variable;
    }

    public SingleVariableCollection(Variable variable, IVariableCollection linkedCollection)
    {
        _variable = variable;
        _linkedCollection = linkedCollection;
    }
    
    public IEnumerator<Variable> GetEnumerator()
    {
        throw new UnreachableException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new UnreachableException();
    }

    public int Count => 1;

    public bool Contains(Variable variable)
    {
        throw new UnreachableException();
    }

    public bool TryAdd(string name, double value)
    {
        throw new UnreachableException();
    }

    public bool TryAdd(Variable variable)
    {
        throw new UnreachableException();
    }

    public void OverrideAdd(string name, double value)
    {
        throw new UnreachableException();
    }

    public void OverrideAdd(Variable variable)
    {
        throw new UnreachableException();
    }

    public bool TryGet(string name, out double result)
    {
        if (_variable.Name == name)
        {
            result = _variable.Value;
            return true;
        }

        return _linkedCollection.TryGet(name, out result);
    }

    public int Remove(string name)
    {
        throw new UnreachableException();
    }

    public void Link(IVariableCollection? variableCollection)
    {
        throw new UnreachableException();
    }
}