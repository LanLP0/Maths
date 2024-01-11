using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Common.Maths;
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

    public int ArgCount => _args.Count;

    /// <summary>
    ///     Skip over a slot (after a comma)
    /// </summary>
    public bool SkipOverSlot { get; set; }

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

        SkipOverSlot = false;
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

    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, Format format, int nodeLevel = 1,
        bool showTree = false, bool latex = false)
    {
        nodeLevel++;
        if (nodeLevel == selectedLevel)
        {
            buffer.Append(Calc(scope).Value.Format(format));
            return Ok();
        }

        if (!latex)
            return Name switch
            {
                "abs" => RenderAbs(buffer, selectedLevel, scope, nodeLevel, showTree, latex, format),
                "sigma" => RenderSigmaCpi(buffer, selectedLevel, scope, nodeLevel, showTree, latex, true, format),
                "cpi" => RenderSigmaCpi(buffer, selectedLevel, scope, nodeLevel, showTree, latex, false, format),
                _ => RenderNormal(buffer, selectedLevel, scope, nodeLevel, showTree, latex, format)
            };

        return Name switch
        {
            "abs" => RenderAbs(buffer, selectedLevel, scope, nodeLevel, showTree, latex, format),
            "sqrt" => RenderSqrtCbrt(buffer, selectedLevel, scope, nodeLevel, showTree, true, format),
            "cbrt" => RenderSqrtCbrt(buffer, selectedLevel, scope, nodeLevel, showTree, false, format),
            "ceiling" => RenderCeilingFloor(buffer, selectedLevel, scope, nodeLevel, showTree, true, format),
            "floor" => RenderCeilingFloor(buffer, selectedLevel, scope, nodeLevel, showTree, false, format),
            "sigma" => RenderSigmaCpi(buffer, selectedLevel, scope, nodeLevel, showTree, latex, true, format),
            "cpi" => RenderSigmaCpi(buffer, selectedLevel, scope, nodeLevel, showTree, latex, false, format),
            _ => RenderNormal(buffer, selectedLevel, scope, nodeLevel, showTree, latex, format)
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

    public IMathNode? GetLastNode()
    {
        if (_args.Count is 0 || SkipOverSlot)
            return null;

        return _args[^1];
    }

    private Result RenderAbs(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel, bool showTree,
        bool latex, Format format)
    {
        var arg = _args[0];

        buffer.Append('|');

        var result = arg.RenderStep(buffer, selectedLevel, scope, format, nodeLevel, showTree, latex);
        if (result.Faulted)
            return result;

        buffer.Append('|');
        return Ok();
    }

    private Result RenderSqrtCbrt(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel,
        bool showTree, bool isSqrt, Format format)
    {
        var arg = _args[0];

        buffer.Append(isSqrt ? @"\sqrt{" : @"\sqrt[3]{");

        var result = arg.RenderStep(buffer, selectedLevel, scope, format, nodeLevel, showTree, true);
        if (result.Faulted)
            return result;

        buffer.Append('}');
        return Ok();
    }

    private Result RenderCeilingFloor(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel,
        bool showTree, bool isCeiling, Format format)
    {
        var arg = _args[0];

        buffer.Append(isCeiling ? @"\lceil " : @"\lfloor ");

        var result = arg.RenderStep(buffer, selectedLevel, scope, format, nodeLevel, showTree, true);
        if (result.Faulted)
            return result;

        buffer.Append(isCeiling ? @"\rceil " : @"\rfloor ");
        return Ok();
    }

    private Result RenderSigmaCpi(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel, bool showTree,
        bool latex, bool isSigma, Format format)
    {
        var args = CollectionsMarshal.AsSpan(_args);

        string variableName;
        int index;
        if (args.Length is 4) // This will always has the variable as the first argument
        {
            variableName = (args[0] as VariableNode)!.Name;
            index = 1;
        }
        else // Grab the variable from the function (index 2)
        {
            args[2].SetupForSolving(scope, out variableName);
            index = 0;
        }

        if (latex)
        {
            buffer.Append(isSigma ? @"\sum_{" : @"\prod_{");
            buffer.Append(variableName);
            buffer.Append('=');
        }
        else
        {
            buffer.Append(isSigma ? "sigma(" : "cpi(");
            buffer.Append(variableName);
            buffer.Append(", ");
        }

        var result = args[index++].RenderStep(buffer, selectedLevel, scope, format, nodeLevel, showTree, latex);
        if (result.Faulted)
            return result;

        buffer.Append(latex ? @"}^{" : ", ");

        result = args[index++].RenderStep(buffer, selectedLevel, scope, format, nodeLevel, showTree, latex);
        if (result.Faulted)
            return result;

        if (latex)
            buffer.Append('}');
        else
            buffer.Append(", ");

        if (latex && args[index] is not (ExponentNode or BitwiseNotNode))
            args[index].Priority = MathTree.ValueNodePriority;

        // This shouldn't be calculated and sometime should be put in braces
        result = args[index].RenderStep(buffer, -1, scope, format, 0, showTree, latex);

        if (!latex)
            buffer.Append(')');
        return result;
    }

    private Result RenderNormal(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel, bool showTree,
        bool latex, Format format)
    {
        buffer.Append(Name);
        buffer.Append('(');

        var args = CollectionsMarshal.AsSpan(_args);
        for (var i = 0; i < args.Length; i++)
        {
            var result = args[i].RenderStep(buffer, selectedLevel, scope, format, nodeLevel, showTree, latex);
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