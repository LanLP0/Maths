// ReSharper disable CommentTypo

using System.Globalization;
using System.Text;
using Common.Results;
using OneOf;

namespace LCalc;

/// <summary>
///     A String Calculator
/// </summary>
public static class Calculator
{
    public static string CalcFormatted(string math)
    {
        var result = CalcRaw(math, out var rawValueRequested, out var steps);

        if (result.IsT0)
            return $"Error: {result.AsT0.Message}";

        if (result.IsT1)
            return $"Result: {result.AsT1}";

        if (steps is not null)
            return steps;

        var result1 = result.AsT2;
        result1 = Math.Round(result1, 6);
        return $"Result: {(rawValueRequested ? result1.ToString(CultureInfo.InvariantCulture) : result1.Humanize())}";
    }

    public static OneOf<Exception, bool, double> CalcRaw(ReadOnlySpan<char> math, out bool rawValueRequested, out string? steps)
    {
        steps = null;
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

        OneOf<Exception, bool, double> result2;
        if (!tree.Scope.GetStepByStepOpt())
            return tree.Calc();
        
        var result1 = CalcStep(tree);
        if (result1.Faulted)
            return result1.Exception!;
        steps = result1.Value;

        return tree.Calc();

    }

    private static Result<string> CalcStep(MathTree.MathTree tree)
    {
        var root = tree.CompareNode ?? tree.Root!;
        var maxDepth = root.GetDepth();
        if (maxDepth.Faulted)
            return maxDepth.Exception!;

        var buffer = new StringBuilder();
        for (var i = maxDepth.Value + 1; i > 0; i--)
        {
            var result = root.RenderStep(buffer, i, tree.Scope, 1, tree.Scope.GetShowTreeOpt());
            if (result.Faulted)
                return result;

            if (i is 1)
                continue;

            if (i is 2 && root.Priority is MathTree.MathTree.ValueNodePriority)
                break;

            buffer.Append(Environment.NewLine);
        }

        return buffer.ToString();
    }
}