using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Common.Results;
using LCalc.Helpers;

namespace LCalc.MathTree.Nodes;

internal sealed class FunctionCallNode : IMathNode
{
    private readonly List<IMathNode> _args = new();

    public FunctionCallNode(string name)
    {
        Name = name;
    }

    public string Name { get; set; }

    public int Priority { get; set; } = MathTree.SpecialNodePriority;

    public Result<double> Calc(Scope scope)
    {
        switch (Name) // Special functions that need the raw IMathNode
        {
            case "sigma":
                return CalculatorHelpers.CalcSigma(_args, scope);
            case "cpi":
                return CalculatorHelpers.CalcCPi(_args, scope);
        }

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

        switch (Name)
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
                var customFunctions = scope.CustomFunctions;
                if (customFunctions is null)
                    return Err($"Unknown function {Name}()");
                return customFunctions.Execute(Name, math);
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
        return Priority != MathTree.SpecialNodePriority;
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
        bool showTree = false, bool latex = false)
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

        if (!latex)
            return Name switch
            {
                "abs" => RenderAbs(buffer, selectedLevel, scope, nodeLevel, showTree, latex),
                "sigma" => RenderSigmaCpi(buffer, selectedLevel, scope, nodeLevel, showTree, latex, true),
                "cpi" => RenderSigmaCpi(buffer, selectedLevel, scope, nodeLevel, showTree, latex, false),
                _ => RenderNormal(buffer, selectedLevel, scope, nodeLevel, showTree, latex)
            };

        return Name switch
        {
            "abs" => RenderAbs(buffer, selectedLevel, scope, nodeLevel, showTree, latex),
            "sqrt" => RenderSqrtCbrt(buffer, selectedLevel, scope, nodeLevel, showTree, true),
            "cbrt" => RenderSqrtCbrt(buffer, selectedLevel, scope, nodeLevel, showTree, false),
            "ceiling" => RenderCeilingFloor(buffer, selectedLevel, scope, nodeLevel, showTree, true),
            "floor" => RenderCeilingFloor(buffer, selectedLevel, scope, nodeLevel, showTree, false),
            "sigma" => RenderSigmaCpi(buffer, selectedLevel, scope, nodeLevel, showTree, latex, true),
            "cpi" => RenderSigmaCpi(buffer, selectedLevel, scope, nodeLevel, showTree, latex, false),
            _ => RenderNormal(buffer, selectedLevel, scope, nodeLevel, showTree, latex)
        };
    }

    public Result<int> GetDepth()
    {
        var limit = Name switch
        {
            "sigma" or "cpi" => 3,
            _ => _args.Count
        };

        var max = 0;
        for (var i = 0; i < limit; i++)
        {
            var arg = _args[i];
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

    private Result RenderAbs(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel, bool showTree,
        bool latex)
    {
        var arg = _args[0];

        buffer.Append('|');

        var result = arg.RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree, latex);
        if (result.Faulted)
            return result;

        buffer.Append('|');
        return Ok();
    }

    private Result RenderSqrtCbrt(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel,
        bool showTree, bool isSqrt)
    {
        var arg = _args[0];

        buffer.Append(isSqrt ? @"\sqrt{" : @"\sqrt[3]{");

        var result = arg.RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree, true);
        if (result.Faulted)
            return result;

        buffer.Append('}');
        return Ok();
    }

    private Result RenderCeilingFloor(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel,
        bool showTree, bool isCeiling)
    {
        var arg = _args[0];

        buffer.Append(isCeiling ? @"\lceil " : @"\lfloor ");

        var result = arg.RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree, true);
        if (result.Faulted)
            return result;

        buffer.Append(isCeiling ? @"\rceil " : @"\rfloor ");
        return Ok();
    }

    private Result RenderSigmaCpi(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel, bool showTree,
        bool latex, bool isSigma)
    {
        var args = CollectionsMarshal.AsSpan(_args);

        if (latex)
        {
            buffer.Append(isSigma ? @"\sum_{" : @"\prod_{");
            buffer.Append((args[0] as VariableNode)!.Name);
            buffer.Append('=');
        }
        else
        {
            buffer.Append(isSigma ? "sigma(" : "cpi(");
            buffer.Append((args[0] as VariableNode)!.Name);
            buffer.Append(", ");
        }

        var result = args[1].RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree, latex);
        if (result.Faulted)
            return result;

        buffer.Append(latex ? @"}^{" : ", ");

        result = args[2].RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree, latex);
        if (result.Faulted)
            return result;

        if (latex)
            buffer.Append('}');
        else
            buffer.Append(", ");

        if (latex && args[3] is not (ExponentNode or BitwiseNotNode))
            args[3].Priority = MathTree.ValueNodePriority;

        // This shouldn't be calculated and sometime should be put in brackets
        result = args[3].RenderStep(buffer, -1, scope, 0, showTree, latex);

        if (!latex)
            buffer.Append(')');
        return result;
    }

    private Result RenderNormal(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel, bool showTree,
        bool latex)
    {
        buffer.Append(Name);
        buffer.Append('(');

        var args = CollectionsMarshal.AsSpan(_args);
        for (var i = 0; i < args.Length; i++)
        {
            var result = args[i].RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree, latex);
            if (result.Faulted)
                return result;

            if (i == args.Length - 1)
                continue;

            buffer.Append(", ");
        }

        buffer.Append(')');
        return Ok();
    }
}