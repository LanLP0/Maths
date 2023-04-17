using Common.Results;
using LCalc.Extension;

namespace LCalc.CustomFunction;

internal sealed class CustomFunction
{
    private MathTree.MathTree _tree;
    
    public bool IsCalled = false;
    
    public Scope Scope { get; private set; }
    public string Name { get; private set; }

    public static Result<CustomFunction> Parse(ReadOnlySpan<char> span, CustomFunctionCollection customFunctions)
    {
        var pos1 = span.IndexOf('=');
        var firstHalf = span[..pos1];
        if (firstHalf[^1] != ')')
            return Err<CustomFunction>("Invalid function signature");
        if (firstHalf.Length <= 2)
            return Err<CustomFunction>("Invalid function signature");

        var pos = firstHalf.IndexOf('(');
        if (pos is -1)
            return Err<CustomFunction>("Invalid function signature");

        var name = firstHalf[..pos].ToString();
        if (name.Length is 0)
            return Err<CustomFunction>("Invalid function name");
        if (customFunctions.ContainsName(name))
            return Err<CustomFunction>("Duplicated functions");
        var argsSpan = firstHalf[(pos + 1)..^1];

        VariableCollection args = new();
        var fn = new CustomFunction();
        if (argsSpan.Length is not 0)
        {
            Span<char> buffer = stackalloc char[argsSpan.Length];
            var count = 0;
            foreach (var chr in argsSpan)
                switch ((int)chr)
                {
                    case > 96 and < 122: // a-z
                    {
                        buffer[count] = chr;
                        count++;
                        break;
                    }
                    case 32: // ' '
                    {
                        if (buffer[0] != '\0')
                            args.TryAdd(buffer.TrimEnd('\0').ToString(), 0);
                        buffer.Clear();
                        count = 0;
                        break;
                    }
                    default:
                        return Err<CustomFunction>($"Invalid character in function '{chr}'");
                }

            if (buffer[0] != '\0')
                args.TryAdd(buffer.TrimEnd('\0').ToString(), 0);
        }

        var secondHalf = span[(pos1 + 1)..];
        if (secondHalf.Length is 0)
            return Err<CustomFunction>("Missing function body");

        fn.Name = name;
        fn.Scope = new Scope(false, false, false, false);
        fn.Scope.Variables = args;
        fn.Scope.CustomFunctions = customFunctions;
        fn._tree = new MathTree.MathTree(fn.Scope);
        var result = fn._tree.Parse(secondHalf);
        if (result.Faulted)
            return result;

        return fn;
    }

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

        var result = _tree.Calc();

        if (result.Faulted)
            return result.Exception!;

        return result.Number!.Value;
    }
}