using System.Text;
using Common.Maths;
using Common.Results;
using LCalc.Helpers;

namespace LCalc.MathTree.Nodes;

internal sealed class BitwiseNotNode : IMathNode
{
    private IMathNode? _arg;
    public int Priority { get; set; } = MathTree.BitwiseNodePriority;

    public Result<double> Calc(Scope scope)
    {
        if (!IsFull())
            return GenerateMissingValueError();

        var result = _arg!.Calc(scope);
        if (result.Faulted)
            return result;
        var result1 = OperatorHelpers.ToInt64(result.Value, "~");
        if (result1.Faulted)
            return result1.Exception!;
        var num = result1.Value;

        return ~num;
    }

    public bool AddNode(IMathNode node)
    {
        if (_arg != null)
            return false;

        _arg = node;
        return true;
    }

    public bool IsFull()
    {
        return _arg != null;
    }

    public void ChangeLastNodeTo(IMathNode node)
    {
        _arg = node;
    }

    public Result GenerateMissingValueError()
    {
        return Err("Missing value for operator ~");
    }


    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, Format format, int nodeLevel = 1,
        bool showTree = false, bool latex = false)
    {
        if (!IsFull())
            return GenerateMissingValueError();

        var isEncased = Priority is MathTree.ValueNodePriority;
        if (isEncased)
            nodeLevel++;

        if (nodeLevel == selectedLevel)
        {
            buffer.Append(Calc(scope).Value.Format(format));
            return Ok();
        }

        if (isEncased || showTree)
            buffer.Append('(');

        if (latex)
            buffer.Append(@" \sim\! ");
        else
            buffer.Append('~');

        var result = _arg!.RenderStep(buffer, selectedLevel, scope, format, nodeLevel, showTree, latex);
        if (result.Faulted)
            return result;

        if (isEncased || showTree)
            buffer.Append(')');

        return Ok();
    }

    public Result<int> GetDepth()
    {
        if (!IsFull())
            return Err();

        var num = _arg!.GetDepth();
        if (num.Faulted)
            return num;

        return num.Value +
            (Priority is MathTree.ValueNodePriority ? 1 : 0);
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        if (IsFull())
            return _arg!.SetupForSolving(scope, out unknown);

        unknown = string.Empty;
        return GenerateMissingValueError();
    }

    public IMathNode? GetLastNode()
    {
        return _arg;
    }
}