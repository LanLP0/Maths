using System.Diagnostics;
using System.Text;
using Common.Results;
using LCalc.Helpers;

namespace LCalc.MathTree.Nodes;

internal sealed class FunctionCallNode : IMathNode
{
    private readonly List<IMathNode> _args = new();
    private string _name = null!;

    public FunctionCallNode(string name)
    {
        _name = name;
    }
    
    public int Priority { get; set; } = MathTree.SpecialNodePriority;

    public Result<double> Calc(Scope scope)
    {
        var customFunctions = scope.CustomFunctions;

        Span<double> math = stackalloc double[_args.Count];
        for (var i = 0; i < _args.Count; i++)
        {
            var arg = _args[i];
            var rs = arg.Calc(scope);
            if (rs.Faulted)
                return rs;

            math[i] = rs.Value;
        }

        Result<double> result;
        switch (_name)
        {
            case "rng":
            case "random":
                if (scope.GetSolveOpt())
                    return Err("Cannot use random() in solve mode");
                result = CalculatorHelpers.CalcRandom(math);
                break;
            case "gcd":
                result = CalculatorHelpers.CalcGcd(math);
                break;
            case "lcm":
                result = CalculatorHelpers.CalcLcm(math);
                break;
            case "sin":
                result = CalculatorHelpers.CalcSin(math);
                break;
            case "cos":
                result = CalculatorHelpers.CalcCos(math);
                break;
            case "tan":
                result = CalculatorHelpers.CalcTan(math);
                break;
            case "cot":
                result = CalculatorHelpers.CalcCot(math);
                break;
            case "sqrt":
                result = CalculatorHelpers.CalcSqrt(math);
                break;
            case "cbrt":
                result = CalculatorHelpers.CalcCbrt(math);
                break;
            case "abs":
                result = CalculatorHelpers.CalcAbs(math);
                break;
            case "log":
                result = CalculatorHelpers.CalcLog(math);
                break;
            case "floor":
                result = CalculatorHelpers.CalcFloor(math);
                break;
            case "ceiling":
                result = CalculatorHelpers.CalcCeiling(math);
                break;
            case "round":
                result = CalculatorHelpers.CalcRound(math);
                break;
            case "avg":
                result = CalculatorHelpers.CalcAvg(math);
                break;
            case "sum":
                result = CalculatorHelpers.CalcSum(math);
                break;
            default:
                if (customFunctions is null)
                    return Err($"Unknown function {_name}()");
                result = customFunctions.Execute(_name, math);
                break;
        }

        if (result.Faulted)
            return result.Exception!;

        return result.Value;
    }

    public bool AddNode(IMathNode node)
    {
        _args.Add(node);

        return true;
    }

    public bool IsFull()
    {
        // This happens when the function call is completed
        if (Priority != MathTree.SpecialNodePriority)
            return true;
        
        return false;
    }

    public void ChangeLastNodeTo(IMathNode node)
    {
        if (_args.Count is 0)
        {
            _args.Add(node);
            return;
        }

        _args[_args.Count - 1] = node;
    }

    public Result GenerateMissingValueError()
    {
        throw new UnreachableException();
    }

    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel = 1,
        bool showTree = false)
    {
        nodeLevel++;
        if (nodeLevel == selectedLevel)
        {
            var result = Calc(scope);
            if (result.Faulted)
                return result;

            buffer.Append(result.Value);
            return Ok();
        }

        buffer.Append(_name);
        buffer.Append('(');

        for (var i = 0; i < _args.Count; i++)
        {
            var result = _args[i].RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree);
            if (result.Faulted)
                return result;

            if (i == _args.Count - 1)
                continue;

            buffer.Append(',');
            buffer.Append(' ');
        }

        buffer.Append(')');

        return Ok();
    }

    public Result<int> GetDepth()
    {
        var max = 0;
        foreach (var arg in _args)
        {
            var result = arg.GetDepth();
            if (result.Faulted)
                return result;

            max = Math.Max(max, result.Value);
        }

        return max + 1;
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        unknown = string.Empty;
        
        foreach (var arg in _args)
        {
            var rs = arg.SetupForSolving(scope, out var unknown1);
            if (unknown1 != string.Empty)
            {
                if (unknown != string.Empty && unknown1 != unknown)
                    return Err("Too many unknowns");

                unknown = unknown1;
            }

            if (rs.Faulted)
                return rs;
        }
        
        return Ok();
    }
}