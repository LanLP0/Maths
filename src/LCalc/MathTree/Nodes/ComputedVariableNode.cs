using System.Diagnostics;
using System.Text;
using Common.Results;

namespace LCalc.MathTree.Nodes;

internal sealed class ComputedVariableNode : IMathNode
{
    private IMathNode? _child;

    public ComputedVariableNode(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public int Priority { get; set; } = MathTree.SpecialNodePriority;

    /// <summary>
    ///     Compute the value of the variable
    /// </summary>
    /// <param name="scope"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public Result<double> Calc(Scope scope)
    {
        if (!IsFull())
            return GenerateMissingValueError();

        return _child!.Calc(scope);
    }

    public bool AddNode(IMathNode node)
    {
        if (_child is not null)
            return false;

        _child = node;
        return true;
    }

    public bool IsFull()
    {
        return _child is not null;
    }

    public void ChangeLastNodeTo(IMathNode node)
    {
        _child = node;
    }

    public Result GenerateMissingValueError()
    {
        return Err("Missing expression in variable assignment");
    }

    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel = 1,
        bool showTree = false,
        bool latex = false)
    {
        throw new UnreachableException();
    }

    public Result<int> GetDepth()
    {
        throw new UnreachableException();
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        throw new UnreachableException();
    }

    public IMathNode? GetLastNode()
    {
        return _child;
    }
}