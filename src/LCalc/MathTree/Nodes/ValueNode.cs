using System.Diagnostics;
using System.Text;
using Common.Results;
using LCalc.Helpers;

namespace LCalc.MathTree.Nodes;

internal sealed class ValueNode : IMathNode
{
    private readonly double _value;

    internal ValueNode(double value)
    {
        _value = value;
    }

    public int Priority { get; set; } = MathTree.ValueNodePriority;

    public Result<double> Calc(Scope scope)
    {
        return _value;
    }

    public bool AddNode(IMathNode node)
    {
        throw new UnreachableException();
    }

    public void ChangeLastNodeTo(IMathNode node)
    {
        throw new UnreachableException();
    }

    public bool IsFull()
    {
        return true;
    }

    public Result GenerateMissingValueError()
    {
        throw new UnreachableException();
    }

    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel = 1,
        bool showTree = false)
    {
        buffer.Append(_value);
        return Ok();
    }

    public Result<int> GetDepth()
    {
        return 1;
    }

    public static Result<ValueNode> Parse(ReadOnlySpan<char> value)
    {
        var e = CalculatorHelpers.Parse(value);
        if (e.Faulted)
            return Err<ValueNode>(e.Exception!);

        return new ValueNode(e.Value!);
    }
}