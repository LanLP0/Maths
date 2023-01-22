using Common.Results;

namespace LCalc.CustomFunction;

internal sealed class CustomFunctionCollection
{
    private readonly List<CustomFunction> _list = new();
    private bool[] _calledFunctions;

    public bool ContainsName(string name)
    {
        return _list.Exists(a => a.Name == name);
    }

    public Result Add(CustomFunction function)
    {
        if (ContainsName(function.Name))
            return Err($"{function.Name}() had already exists");

        _list.Add(function);
        return Ok();
    }

    public void End(Dictionary<string, double>? additionalArgs)
    {
        _calledFunctions = new bool[_list.Count];

        if (additionalArgs == null)
            return;

        foreach (var fn in _list)
        foreach (var arg in additionalArgs)
            fn.Args.TryAdd(arg.Key, arg.Value);
    }

    public Result<double> Execute(string name, scoped ReadOnlySpan<double> math)
    {
        if (_list.Count is 0)
            return Err($"Unknown function {name}()");

        var pos = _list.FindIndex(a => a.Name == name);
        if (pos is -1)
            return Err($"Unknown function {name}()");

        if (_calledFunctions[pos])
            return Err("Cannot call a function in it-self");
        var function = _list[pos];

        _calledFunctions[pos] = true;
        var result = function.Run(math);
        _calledFunctions[pos] = false;
        return result;
    }
}