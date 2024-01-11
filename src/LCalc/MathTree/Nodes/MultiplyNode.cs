using System.Text;
using Common.Maths;
using Common.Results;

namespace LCalc.MathTree.Nodes;

internal sealed class MultiplyNode : IMathNode
{
    private IMathNode? _arg1;
    private IMathNode? _arg2;
    public int Priority { get; set; } = MathTree.MulDivModNodePriority;

    public Result<double> Calc(Scope scope)
    {
        if (!IsFull())
            return GenerateMissingValueError();

        var result = _arg1!.Calc(scope);
        if (result.Faulted)
            return result;
        var num1 = result.Value;

        result = _arg2!.Calc(scope);
        if (result.Faulted)
            return result;
        var num2 = result.Value;

        return num1 * num2;
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
        if (_arg1 is null)
        {
            _arg1 = node;
            return;
        }

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
            return Err("Missing value before operator *");

        return Err("Missing value after operator *");
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

        var result = _arg1!.RenderStep(buffer, selectedLevel, scope, format, nodeLevel, showTree, latex);
        if (result.Faulted)
            return result;

        buffer.Append(latex ? " \\times " : " * ");

        result = _arg2!.RenderStep(buffer, selectedLevel, scope, format, nodeLevel, showTree, latex);
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
            (Priority is MathTree.ValueNodePriority ? 1 : 0);
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
        if (unknown == string.Empty)
            return _arg2!.SetupForSolving(scope, out unknown);

        _arg2!.SetupForSolving(scope, out var unknown1);
        if (unknown1 != string.Empty && unknown1 != unknown)
            return Err("Too many unknowns");

        return rs;
    }

    public IMathNode? GetLastNode()
    {
        if (_arg2 is not null)
            return _arg2;

        return _arg1;
    }
}