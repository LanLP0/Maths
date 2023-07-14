using System.Collections;
using System.Diagnostics;

namespace LCalc;

/// <summary>
/// Used by sigma() or cpi() to efficiently override a single variable
/// </summary>
internal sealed class SigmaCPiVariableCollection : IVariableCollection
{
    private readonly IVariableCollection _linkedCollection;

    public SigmaCPiVariableCollection(Variable variable, IVariableCollection linkedCollection)
    {
        Variable = variable;
        _linkedCollection = linkedCollection;
    }

    public Variable Variable { get; }

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

    public bool Contains(string name)
    {
        // We want this able to be the unknown to allow
        // overriding the default value in _linkedCollection
        if (name == Variable.Name)
            return false;

        return _linkedCollection.Contains(name);
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
        if (Variable.Name == name)
        {
            result = Variable.Value;
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