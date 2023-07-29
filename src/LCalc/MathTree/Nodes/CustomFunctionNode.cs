using System.Diagnostics;
using System.Text;
using Common.Results;
using LCalc.CustomFunction;
using LCalc.Variables;

namespace LCalc.MathTree.Nodes;

internal sealed class CustomFunctionNode : IMathNode
{
    private readonly string _name;
    private readonly VariableCollection _variables;
    private IMathNode? _node;

    public CustomFunctionNode(string name, VariableCollection variables)
    {
        _name = name;
        _variables = variables;
    }

    public int Priority { set; get; } = MathTree.SpecialNodePriority;

    public Result<double> Calc(Scope scope)
    {
        throw new UnreachableException();
    }

    public bool AddNode(IMathNode node)
    {
        if (IsFull())
            return false;

        _node = node;
        return true;
    }

    public bool IsFull()
    {
        return _node is not null;
    }

    public void ChangeLastNodeTo(IMathNode node)
    {
        _node = node;
    }

    public Result GenerateMissingValueError()
    {
        throw new UnreachableException();
    }

    public Result RenderStep(StringBuilder buffer, int selectedLevel, Scope scope, int nodeLevel = 1,
        bool showTree = false)
    {
        throw new UnreachableException();
    }

    public Result<int> GetDepth()
    {
        throw new UnreachableException();
    }

    public Result SetupForSolving(Scope scope, out string unknown)
    {
        throw new UnreachableException();
    }

    public CustomFunction.CustomFunction ToCustomFunction(CustomFunctionCollection customFunctions)
    {
        var scope = new Scope(false);
        scope.Variables = _variables;
        scope.CustomFunctions = customFunctions;
        var fn = new CustomFunction.CustomFunction(scope, _name, _node!);
        return fn;
    }
}