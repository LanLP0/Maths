
namespace LCalc.CustomFunction;

internal class CustomFunctionCollection
{
    private readonly List<CustomFunction> _list = new();
    private bool[] _calledFunction = Array.Empty<bool>();

    public bool ContainsName(string name) => _list.Exists(a => a.Name == name);
    
    public void Add(CustomFunction function) => _list.Add(function);

    public void End()
    {
        _calledFunction = new bool[_list.Count];
    }

    public Result Execute(string name, List<CalcElement> math)
    {
        if (_list.Count is 0)
            return Err($"Unknown function: {name}()");
        
        var pos = _list.FindIndex(a => a.Name == name);
        if (pos is -1)
            return Err($"Unknown function: {name}()");
        
        if (_calledFunction[pos])
            return Err("Cannot call a function in it-self");
        _calledFunction[pos] = true;
        
        var function = _list[pos];

        var result = function.Run(math, this);
        _calledFunction[pos] = false;
        return result;
    }
}