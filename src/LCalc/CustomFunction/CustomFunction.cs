using Common.Results;
using LCalc.Extension;

namespace LCalc.CustomFunction;

internal sealed class CustomFunction
{
    private Scope _scope;
    private MathTree.MathTree _tree;
    public VariableCollection Args;

    public bool IsCalled = false;
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
            return Err<CustomFunction>("Duplicate functions");
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
        fn.Args = args;
        fn._scope = customFunctions.Scope;
        fn._scope.Variables = args;
        fn._scope.CustomFunctions = customFunctions;
        fn._tree = new MathTree.MathTree(fn._scope);
        var result = fn._tree.Parse(secondHalf);
        if (result.Faulted)
            return result;

        return fn;
    }

    public Result<double> Run(scoped ReadOnlySpan<double> args)
    {
        if (args.Length != Args.Count)
            return Err("Invalid number of args");

        var i = 0;
        foreach (var arg in Args)
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