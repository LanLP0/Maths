using System.Diagnostics;
using System.Text;
using Common.Results;
using LCalc.Helpers;

namespace LCalc.MathTree.Nodes;

internal sealed class ValueNode : IMathNode
{
    public double Value { get; set; }

    internal ValueNode(double value)
    {
        Value = value;
    }

    public int Priority { get; set; } = MathTree.ValueNodePriority;

    public Result<double> Calc(Scope scope)
    {
        return Value;
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
        bool showTree = false, bool latex = false)
    {
        buffer.Append(Value);
        return Ok();
    }

    public Result<int> GetDepth()
    {
        return 1;
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        unknown = string.Empty;
        return Ok();
    }

    public IMathNode? GetLastNode()
    {
        return null;
    }

    public static Result<ValueNode> Parse(ReadOnlySpan<char> value)
    {
        var e = CalculatorHelpers.ParseNumber(value);
        if (e.Faulted)
            return Err<ValueNode>(e.Exception!);

        return new ValueNode(e.Value);
    }
}