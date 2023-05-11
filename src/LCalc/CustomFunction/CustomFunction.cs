using Common.Results;
using LCalc.MathTree;

namespace LCalc.CustomFunction;

internal sealed class CustomFunction
{
    private readonly IMathNode _node;

    public CustomFunction(Scope scope, string name, IMathNode node)
    {
        Scope = scope;
        Name = name;
        _node = node;
    }

    public bool IsCalled { get; set; }
    public Scope Scope { get; }
    public string Name { get; }

    public Result<double> Run(scoped ReadOnlySpan<double> args)
    {
        if (args.Length != Scope.Variables.Count)
            return Err("Invalid number of args");

        var i = 0;
        foreach (var arg in Scope.Variables)
        {
            arg.SetValue(args[i]);
            i++;
        }

        // var result = _tree.Calc();
        var result = _node.Calc(Scope);

        if (result.Faulted)
            return result.Exception!;

        return result.Value;
    }
}