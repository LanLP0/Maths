using System.Diagnostics;
using System.Text;
using Common.Results;

namespace LCalc.MathTree.Nodes;

internal sealed class VariableNode : IMathNode
{
    private readonly string _name;

    public string Name => _name;

    public VariableNode(string name)
    {
        _name = name;
    }

    public int Priority { get; set; } = MathTree.ValueNodePriority;

    public Result<double> Calc(Scope scope)
    {
        var result = scope.GetVariable(_name);
        if (result.Faulted)
            return result;

        return result.Value;
    }

    public bool AddNode(IMathNode node)
    {
        throw new UnreachableException();
    }

    public bool IsFull()
    {
        return true;
    }

    public void ChangeLastNodeTo(IMathNode node)
    {
        throw new UnreachableException();
    }

    public Result GenerateMissingValueError()
    {
        throw new UnreachableException();
    }

    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel = 1,
        bool showTree = false)
    {
        var result = Calc(scope);
        if (result.Faulted)
            return result;

        buffer.Append(result.Value);
        return Ok();
    }

    public Result<int> GetDepth()
    {
        return 1;
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        var result = scope.GetVariable(_name);
        if (result.Faulted)
        {
            unknown = _name;
            return Ok();
        }

        unknown = string.Empty;
        return Ok();
    }
}