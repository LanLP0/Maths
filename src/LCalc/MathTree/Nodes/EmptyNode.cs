using System.Diagnostics;
using System.Text;
using Common.Results;

namespace LCalc.MathTree.Nodes;

internal sealed class EmptyNode : IMathNode
{
    public static EmptyNode Shared = new();

    public int Priority { get; set; } = MathTree.ValueNodePriority;

    public Result<double> Calc(Scope scope)
    {
        return 0.0;
    }

    public bool AddNode(IMathNode node)
    {
        throw new UnreachableException();
    }

    public bool IsFull()
    {
        return true;
    }

    public void ChangeLastNodeTo(IMathNode node)
    {
        throw new UnreachableException();
    }

    public Result GenerateMissingValueError()
    {
        throw new UnreachableException();
    }

    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel = 1,
        bool showTree = false)
    {
        return Ok();
    }

    public Result<int> GetDepth()
    {
        return 1;
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        unknown = string.Empty;
        return Ok();
    }
}