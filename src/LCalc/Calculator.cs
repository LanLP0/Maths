using System.Text;
using Common.Results;
using LCalc.Extension;
using LCalc.MathTree;

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
    /// <param name="options">Calculator options (will override options set within the math string)</param>
    /// <param name="prevAns">The previous answer (variable: ans)</param>
    /// <returns>The result (formatted)</returns>
    public static string CalcFormatted(string math, CalculatorOption options = CalculatorOption.None,
        double prevAns = double.NaN)
    {
        return CalcFormatted((ReadOnlySpan<char>)math, options, prevAns);
    }

    /// <summary>
    ///     Calculate the expression
    /// </summary>
    /// <param name="math">The expression</param>
    /// <param name="options">Calculator options (will override options set within the math string)</param>
    /// <param name="prevAns">The previous answer (variable: ans)</param>
    /// <returns>The result (formatted)</returns>
    public static string CalcFormatted(ReadOnlySpan<char> math, CalculatorOption options = CalculatorOption.None,
        double prevAns = double.NaN)
    {
        var result = CalcRaw(math, out var rawValueRequested, options, prevAns);

        return result.Render(rawValueRequested);
    }

    /// <summary>
    ///     Calculate the expression
    /// </summary>
    /// <param name="math">The expression</param>
    /// <param name="rawValueRequested">If the &raw argument exists</param>
    /// <param name="options">Calculator options (will override options set within the math string)</param>
    /// <param name="prevAns">The previous answer (variable: ans)</param>
    /// <returns>The raw result</returns>
    public static CalcResult CalcRaw(ReadOnlySpan<char> math, out bool rawValueRequested,
        CalculatorOption options = CalculatorOption.None, double prevAns = double.NaN)
    {
        rawValueRequested = false;

        if (math.Length is 0)
            return new Exception("No expression found");

        Span<char> math1 = stackalloc char[math.Length];
        math.ToLowerInvariant(math1);

        var scope = options != CalculatorOption.None ? new Scope(options) : new Scope();

        var tree = new MathTree.MathTree(scope);
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

        var treeOpt = tree.Scope.GetShowTreeOpt();
        var latex = tree.Scope.GetLaTeXOpt();
        var latexDoc = tree.Scope.GetLaTeXDocOpt();

        var buffer = new StringBuilder();
        if (latexDoc)
            buffer.Append(
                """
                \documentclass{article}
                \usepackage{amsmath}
                \begin{document}
                \begin{gather*}
                
                """);

        for (var i = maxDepth.Value + 1; i > 1; i--)
        {
            var result = root.RenderStep(buffer, i, tree.Scope, 1, treeOpt, latex);
            if (result.Faulted)
                return result;

            if (i is 2)
                continue;

            // Prevent the last line to be printed twice
            if (i is 3 && root.Priority is MathTree.MathTree.ValueNodePriority)
                break;

            if (latex)
                buffer.Append("\\\\");
            buffer.Append(Environment.NewLine);
        }

        if (root is not MathComparer)
        {
            if (latexDoc)
                buffer.Append(
                    """
                    
                    \end{gather*}
                    \end{document}
                    """);

            return buffer.ToString();
        }

        if (latex)
            buffer.Append("\\\\");
        buffer.Append(Environment.NewLine);

        root.RenderStep(buffer, 1, tree.Scope, 1, treeOpt, latex);

        // There is a duplicate
        if (CheckLastTwo(buffer, latex, out var s))
        {
            var index = s.LastIndexOf(latex ? "\\\\" : Environment.NewLine, StringComparison.Ordinal);

            buffer.Remove(index, buffer.Length - index);
        }

        if (latexDoc)
            buffer.Append(
                """
                
                \end{gather*}
                \end{document}
                """);

        return buffer.ToString();

        bool CheckLastTwo(StringBuilder buffer, bool latex, out string s)
        {
            s = buffer.ToString();
            var lines = s.Split(Environment.NewLine);

            var line1 = lines[^2];
            var line2 = lines[^1];

            if (latex) return line1 == line2 + "\\\\";

            return line1 == line2;
        }
    }
}