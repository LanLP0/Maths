using System.Text;
using Common.Results;
using LCalc.Helpers;

namespace LCalc.MathTree.Nodes;

internal sealed class BitwiseOrNode : IMathNode
{
    private IMathNode? _arg1;
    private IMathNode? _arg2;
    public int Priority { get; set; } = MathTree.BitwiseNodePriority;

    public Result<double> Calc(Scope scope)
    {
        if (!IsFull())
            return GenerateMissingValueError();

        var result = _arg1!.Calc(scope);
        if (result.Faulted)
            return result;
        var result1 = OperatorHelpers.ToInt64(result.Value, "|");
        if (result1.Faulted)
            return result1.Exception!;
        var num1 = result1.Value;

        result = _arg2!.Calc(scope);
        if (result.Faulted)
            return result;
        result1 = OperatorHelpers.ToInt64(result.Value, "|");
        if (result1.Faulted)
            return result1.Exception!;
        var num2 = result1.Value;

        return num1 | num2;
    }

    public bool AddNode(IMathNode node)
    {
        if (_arg1 is null)
        {
            _arg1 = node;
            return true;
        }

        if (_arg2 != null)
            return false;

        _arg2 = node;
        return true;
    }

    public bool IsFull()
    {
        return _arg2 != null;
    }

    public void ChangeLastNodeTo(IMathNode node)
    {
        if (_arg2 is null)
        {
            _arg1 = node;
            return;
        }

        _arg2 = node;
    }

    public Result GenerateMissingValueError()
    {
        if (_arg1 is null)
            return Err("Missing value before operator |");

        return Err("Missing value after operator |");
    }


    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel = 1,
        bool showTree = false)
    {
        if (!IsFull())
            return GenerateMissingValueError();

        var isEncased = Priority == MathTree.ValueNodePriority;
        if (isEncased)
            nodeLevel++;

        if (nodeLevel == selectedLevel)
        {
            buffer.Append(Calc(scope).Value);
            return Ok();
        }

        if (isEncased || showTree)
            buffer.Append('(');

        var result = _arg1!.RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree);
        if (result.Faulted)
            return result;
        buffer.Append(' ');
        buffer.Append('|');
        buffer.Append(' ');
        result = _arg2!.RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree);
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

        var num1 = _arg1!.GetDepth();
        if (num1.Faulted)
            return num1;

        var num2 = _arg2!.GetDepth();
        if (num2.Faulted)
            return num2;

        return Math.Max(num1.Value, num2.Value) +
               (Priority == MathTree.ValueNodePriority ? 1 : 0);
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        if (!IsFull())
        {
            unknown = string.Empty;
            return GenerateMissingValueError();
        }

        var rs = _arg1!.SetupForSolving(scope, out unknown);
        if (rs.Faulted)
            return rs;
        if (unknown != string.Empty)
        {
            _arg2!.SetupForSolving(scope, out var unknown1);
            if (unknown1 != string.Empty && unknown1 != unknown)
                return Err("Too many unknowns");

            return rs;
        }

        return _arg2!.SetupForSolving(scope, out unknown);
    }
}