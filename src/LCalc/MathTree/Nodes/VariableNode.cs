using System.Diagnostics;
using System.Text;
using Common.Maths;
using Common.Results;

namespace LCalc.MathTree.Nodes;

internal sealed class VariableNode : IMathNode
{
    public VariableNode(string name)
    {
        Name = name;
    }

    public string Name { get; }

    public int Priority { get; set; } = MathTree.ValueNodePriority;

    public Result<double> Calc(Scope scope)
    {
        var result = scope.GetVariable(Name);
        if (result.Faulted)
            return result;

        return result.Value;
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

    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, Format format, int nodeLevel = 1,
        bool showTree = false, bool latex = false)
    {
        // This allow for variable to be rendered in sigma() or cpi()
        if (selectedLevel is -1)
        {
            buffer.Append(Name);
            return Ok();
        }

        buffer.Append(Calc(scope).Value.Format(format));
        return Ok();
    }

    public Result<int> GetDepth()
    {
        return 1;
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        if (!scope.Variables.Contains(Name))
        {
            unknown = Name;
            return Ok();
        }

        unknown = string.Empty;
        return Ok();
    }

    public IMathNode? GetLastNode()
    {
        return null;
    }
}