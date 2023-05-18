using System.Text;
using Common.Results;

namespace LCalc.MathTree.Nodes;

internal sealed class FactorialNode : IMathNode
{
    private IMathNode? _arg;
    public int Priority { get; set; } = MathTree.ExpFacNodePriority;

    public Result<double> Calc(Scope scope)
    {
        if (!IsFull())
            return GenerateMissingValueError();

        var result = _arg!.Calc(scope);
        if (result.Faulted)
            return result;
        if (!result.Value.IsInt())
            return Err<double>("Operator ! only accept integer value");
        var num = result.Value;
        if (num > 170)
            return double.PositiveInfinity;

        var initialValue = num;

        if (num < 0)
            return Err<double>("Operator ! only accept positive value");

        // Special case 0! = 1
        if (num < double.Epsilon) // == 0
            return 1;

        for (long i = 2; i < initialValue; i++) num *= i;

        return num;
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
        return Err("Missing value for operator !");
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

        var result = _arg!.RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree);
        if (result.Faulted)
            return result;
        buffer.Append('!');

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
               (Priority == MathTree.ValueNodePriority ? 1 : 0);
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        if (IsFull())
            return _arg!.SetupForSolving(scope, out unknown);
        
        unknown = string.Empty;
        return GenerateMissingValueError();
    }
}