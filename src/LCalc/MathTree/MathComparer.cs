using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Common.Results;

namespace LCalc.MathTree;

// Not actually a node
// IMathNode is used for convenience
internal sealed class MathComparer : IMathNode
{
    private readonly List<IMathNode> _args = new();
    private readonly List<CompareOp> _compareOps = new();

    public int Priority { get; set; } = MathTree.SpecialNodePriority;

    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel = 1,
        bool showTree = false, bool latex = false)
    {
        var args = CollectionsMarshal.AsSpan(_args);
        for (var i = 0; i < args.Length; i++)
        {
            if (selectedLevel is 1)
            {
                var arg = args[i];
                if (arg.Priority is not MathTree.ValueNodePriority)
                {
                    var result = arg.RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree, latex);
                    if (result.Faulted)
                        return result;
                }
                else
                {
                    buffer.Append(arg.Calc(scope).Value);
                }
            }
            else
            {
                var result = args[i].RenderStep(buffer, selectedLevel, scope, nodeLevel, showTree, latex);
                if (result.Faulted)
                    return result;
            }

            if (i >= _args.Count - 1)
                continue;

            var op = latex
                ? _compareOps[i] switch
                {
                    CompareOp.Equal => "=",
                    CompareOp.Difference => "\\neq",
                    CompareOp.GreaterThanOrEqual => "\\ge",
                    CompareOp.LessThanOrEqual => "\\le",
                    CompareOp.GreaterThan => ">",
                    CompareOp.LessThan => "<",
                    _ => throw new ArgumentOutOfRangeException()
                }
                : _compareOps[i] switch
                {
                    CompareOp.Equal => "==",
                    CompareOp.Difference => "!=",
                    CompareOp.GreaterThanOrEqual => ">=",
                    CompareOp.LessThanOrEqual => "<=",
                    CompareOp.GreaterThan => ">",
                    CompareOp.LessThan => "<",
                    _ => throw new UnreachableException()
                };

            buffer.Append(' ');
            buffer.Append(op);
            buffer.Append(' ');
        }

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

        return max;
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        if (_compareOps is not [CompareOp.Equal] || _args.Count is not 2)
        {
            unknown = string.Empty;
            return Err("Invalid solve syntax");
        }

        var rs = _args[0].SetupForSolving(scope, out unknown);
        if (rs.Faulted)
            return rs;
        
        if (unknown == string.Empty)
            return _args[1].SetupForSolving(scope, out unknown);

        _args[1].SetupForSolving(scope, out var unknown1);
        if (unknown1 != string.Empty && unknown1 != unknown)
            return Err("Too many unknowns");

        return rs;
    }

    bool IMathNode.AddNode(IMathNode node)
    {
        throw new NotImplementedException();
    }

    public bool IsFull()
    {
        throw new NotImplementedException();
    }

    public void ChangeLastNodeTo(IMathNode node)
    {
        throw new NotImplementedException();
    }

    // Used only in the solver
    Result<double> IMathNode.Calc(Scope scope)
    {
        if (!scope.GetSolveOpt())
            throw new InvalidOperationException();

        var rs = _args[0].Calc(scope);
        if (rs.Faulted)
            return rs;
        
        var num1 = rs.Value;

        rs = _args[1].Calc(scope);
        if (rs.Faulted)
            return rs;

        return num1 - rs.Value;
    }

    public Result GenerateMissingValueError()
    {
        throw new NotImplementedException();
    }

    internal Result<bool> Calc(Scope scope)
    {
        var args = CollectionsMarshal.AsSpan(_args);

        if (args.Length <= 1)
            return Err<bool>("Missing value");

        if (_compareOps.Count >= args.Length)
            return Err<bool>("Missing value");

        for (var i = 0; i < _compareOps.Count; i++)
        {
            var result = args[i].Calc(scope);
            if (result.Faulted)
                return result.Exception!;
            var num1 = result.Value;

            result = args[i + 1].Calc(scope);
            if (result.Faulted)
                return result.Exception!;
            var num2 = result.Value;

            var rs = _compareOps[i] switch
            {
                CompareOp.Equal => Math.Abs(num1 - num2) < double.Epsilon,
                CompareOp.Difference => Math.Abs(num1 - num2) > double.Epsilon,
                CompareOp.GreaterThanOrEqual => num1 >= num2,
                CompareOp.LessThanOrEqual => num1 <= num2,
                CompareOp.GreaterThan => num1 > num2,
                CompareOp.LessThan => num1 < num2,
                _ => throw new UnreachableException()
            };

            if (!rs)
                return false;
        }

        return true;
    }

    internal void AddNode(IMathNode node)
    {
        _args.Add(node);
    }

    internal void AddOp(CompareOp op)
    {
        _compareOps.Add(op);
    }

    internal void Clear()
    {
        _compareOps.Clear();
        _args.Clear();
    }
}

internal enum CompareOp
{
    Equal,
    Difference,
    GreaterThanOrEqual,
    LessThanOrEqual,
    GreaterThan,
    LessThan
}