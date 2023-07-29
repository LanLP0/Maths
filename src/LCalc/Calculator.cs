using System.Text;
using Common.Results;
using LCalc.Extension;
using LCalc.MathTree;
using LCalc.MathTree.Nodes;

namespace LCalc;

/// <summary>
///     A String Calculator
/// </summary>
public static class Calculator
{
    /// <summary>
    ///     Calculate the expression
    /// </summary>
    /// <param name="math">The expression</param>
    /// <param name="steps">Render solving steps (result.Steps)</param>
    /// <param name="prevAns">The previous answer (variable: ans)</param>
    /// <returns>The result (formatted)</returns>
    public static string CalcFormatted(string math, bool steps = false, double prevAns = double.NaN)
    {
        return CalcFormatted((ReadOnlySpan<char>)math, steps, prevAns);
    }

    /// <summary>
    ///     Calculate the expression
    /// </summary>
    /// <param name="math">The expression</param>
    /// <param name="steps">Render solving steps (result.Steps)</param>
    /// <param name="prevAns">The previous answer (variable: ans)</param>
    /// <returns>The result (formatted)</returns>
    public static string CalcFormatted(ReadOnlySpan<char> math, bool steps = false, double prevAns = double.NaN)
    {
        var result = CalcRaw(math, out var rawValueRequested, steps, prevAns);

        return result.Render(rawValueRequested);
    }

    /// <summary>
    ///     Calculate the expression
    /// </summary>
    /// <param name="math">The expression</param>
    /// <param name="rawValueRequested">If the &raw argument exists</param>
    /// <param name="steps">Render solving steps (result.Steps)</param>
    /// <param name="prevAns">The previous answer (variable: ans)</param>
    /// <returns>The raw result</returns>
    public static CalcResult CalcRaw(ReadOnlySpan<char> math, out bool rawValueRequested,
        bool steps = false, double prevAns = double.NaN)
    {
        rawValueRequested = false;

        if (math.Length is 0)
            return new Exception("No expression found");

        Span<char> math1 = stackalloc char[math.Length];
        math.ToLowerInvariant(math1);

        var tree = new MathTree.MathTree();
        var result = tree.Parse(math1);
        if (result.Faulted)
            return result.Exception!;
        rawValueRequested = tree.Scope.GetRawValueOpt();

        if (tree.Scope.GetSolveOpt())
        {
            var node = tree.GetTopNode();
            var rs = node.SetupForSolving(tree.Scope, out var unknown);
            if (rs.Faulted)
                return rs.Exception!;

            if (unknown == string.Empty)
                return CalcResult.Err("Nothing to solve for");

            return NewtonRaphsonSolver.SolveFor(tree, unknown).MapToCalcResult();
        }
        
        // Override if needed
        if (steps)
            tree.Scope.SetStepByStepOpt();

        if (!double.IsNaN(prevAns))
            tree.Scope.SetVariable("ans", prevAns);

        var calcResult = tree.Calc();
        if (!tree.Scope.GetStepByStepOpt() || calcResult.Faulted)
            return calcResult;

        var result1 = CalcStep(tree);
        if (result1.Faulted)
            return result1.Exception!;

        return calcResult.WithSteps(result1.Value!);
    }

    private static Result<string> CalcStep(MathTree.MathTree tree)
    {
        var root = tree.GetTopNode();
        var maxDepth = root.GetDepth();
        if (maxDepth.Faulted)
            return maxDepth.Exception!;

        var buffer = new StringBuilder();
        for (var i = maxDepth.Value + 1; i > 1; i--)
        {
            var result = root.RenderStep(buffer, i, tree.Scope, 1, tree.Scope.GetShowTreeOpt());
            if (result.Faulted)
                return result;

            if (i is 2)
                continue;

            // Prevent the last line to be printed twice
            if (i is 3 && root.Priority is MathTree.MathTree.ValueNodePriority)
                break;

            buffer.Append(Environment.NewLine);
        }

        if (root is not MathComparer)
            return buffer.ToString();

        buffer.Append(Environment.NewLine);
        root.RenderStep(buffer, 1, tree.Scope, 1, tree.Scope.GetShowTreeOpt());

        return buffer.ToString();
    }
}