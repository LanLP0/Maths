using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Common.Results;
using LCalc.Helpers;

namespace LCalc.MathTree.Nodes;

internal sealed class FunctionCallNode : IMathNode
{
    private readonly List<IMathNode> _args = new();
    private readonly string _name;

    public FunctionCallNode(string name)
    {
        _name = name;
    }

    public int Priority { get; set; } = MathTree.SpecialNodePriority;

    public Result<double> Calc(Scope scope)
    {
        switch (_name) // Special functions that need the raw IMathNode
        {
            case "sigma":
                return CalculatorHelpers.CalcSigma(_args, scope);
            case "cpi":
                return CalculatorHelpers.CalcCPi(_args, scope);
        }
        var customFunctions = scope.CustomFunctions;

        var args = CollectionsMarshal.AsSpan(_args);
        Span<double> math = stackalloc double[args.Length];
        for (var i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var rs = arg.Calc(scope);
            if (rs.Faulted)
                return rs;

            math[i] = rs.Value;
        }

        switch (_name)
        {
            case "rng":
            case "random":
                if (scope.GetSolveOpt())
                    return Err("Cannot use random() in solve mode");
                return CalculatorHelpers.CalcRandom(math);
            case "gcd":
                return CalculatorHelpers.CalcGcd(math);
            case "lcm":
                return CalculatorHelpers.CalcLcm(math);
            case "sin":
                return CalculatorHelpers.CalcSin(math);
            case "cos":
                return CalculatorHelpers.CalcCos(math);
            case "tan":
                return CalculatorHelpers.CalcTan(math);
            case "cot":
                return CalculatorHelpers.CalcCot(math);
            case "sqrt":
                return CalculatorHelpers.CalcSqrt(math);
            case "cbrt":
                return CalculatorHelpers.CalcCbrt(math);
            case "abs":
                return CalculatorHelpers.CalcAbs(math);
            case "log":
                return CalculatorHelpers.CalcLog(math);
            case "floor":
                return CalculatorHelpers.CalcFloor(math);
            case "ceiling":
                return CalculatorHelpers.CalcCeiling(math);
            case "round":
                return CalculatorHelpers.CalcRound(math);
            case "avg":
                return CalculatorHelpers.CalcAvg(math);
            case "sum":
                return CalculatorHelpers.CalcSum(math);
            default:
                if (customFunctions is null)
                    return Err($"Unknown function {_name}()");
                return customFunctions.Execute(_name, math);
        }
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

        var args = CollectionsMarshal.AsSpan(_args);
        for (var i = 0; i < args.Length; i++)
        {
            var result = args[i].RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree);
            if (result.Faulted)
                return result;

            if (i == args.Length - 1)
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