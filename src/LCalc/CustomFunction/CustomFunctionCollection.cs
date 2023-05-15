using Common.Results;

namespace LCalc.CustomFunction;

internal sealed class CustomFunctionCollection
{
    private const int MaxFunctionAmount = 15;
    private readonly List<CustomFunction> _list = new();

    public bool ContainsName(string name)
    {
        foreach (var fn in _list)
            if (fn.Name == name)
                return true;

        return false;
    }

    private CustomFunction? GetFunction(string name)
    {
        foreach (var fn in _list)
            if (fn.Name == name)
                return fn;

        return null;
    }

    public Result Add(CustomFunction function)
    {
        if (_list.Count >= MaxFunctionAmount)
            return Err("Max custom function amount reached");

        if (ContainsName(function.Name))
            return Err($"{function.Name}() had already exists");

        _list.Add(function);
        return Ok();
    }

    public void End(IVariableCollection? additionalArgs, CalculatorOption option)
    {
        if (additionalArgs is null)
            return;

        foreach (var fn in _list)
        {
            fn.Scope.Variables.Link(additionalArgs);
            fn.Scope.Option = option;
        }
    }

    public Result<double> Execute(string name, scoped ReadOnlySpan<double> math)
    {
        if (_list.Count is 0)
            return Err($"Unknown function {name}()");

        var fn = GetFunction(name);
        if (fn is null)
            return Err($"Unknown function {name}()");

        if (fn.IsCalled)
            return Err("Function loop is not allowed");

        fn.IsCalled = true;
        var result = fn.Run(math);
        fn.IsCalled = false;

        return result;
    }
}