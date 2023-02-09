using Common.Results;
using LCalc.CustomFunction;

namespace LCalc;

internal sealed class Scope
{
    private readonly bool _isCustomFunctionAllowed;
    private readonly bool _isVariableAllowed;

    [Obsolete("Please use Scope.Create", true)]
    public Scope()
    {
        throw new Exception("Please use Scope.Create");
    }

    private Scope(bool isCustomFunctionAllowed, bool isCalculatorOptionAllowed, bool isVariableAllowed,
        bool isCompareAllowed)
    {
        _isCustomFunctionAllowed = isCustomFunctionAllowed;
        IsCalculatorOptionAllowed = isCalculatorOptionAllowed;
        _isVariableAllowed = isVariableAllowed;
        IsCompareAllowed = isCompareAllowed;

        if (isCalculatorOptionAllowed)
            Options = new CalculatorOptions();
        if (isCustomFunctionAllowed)
            CustomFunctions = new CustomFunctionCollection();
        Variables = new VariableCollection();
    }

    private CalculatorOptions? Options { get; }
    public CustomFunctionCollection? CustomFunctions { get; set; }
    public VariableCollection Variables { get; set; }
    public bool IsCompareAllowed { get; }
    public bool IsCalculatorOptionAllowed { get; }

    public Result<double> GetVariable(string name)
    {
        if (Variables.TryGet(name, out var variable))
            return variable;

        if (!_isVariableAllowed) // Allow for some standard variable like: pi
            return Err<double>("Variable not allowed in this scope");

        return Err<double>($"Unknown variable '{name}'");
    }

    public Result SetVariable(string name, double value)
    {
        if (!_isVariableAllowed)
            return Err("Variable not allowed in this scope");

        if (Variables.TryAdd(name, value))
            return Ok();

        return Err($"Variable '{name}' had already been set");
    }

    public Result SetStepByStepOpt(bool value)
    {
        if (!IsCalculatorOptionAllowed)
            return Err("Cannot set calculator option in this scope");

        Options!.StepByStep = value;
        return Ok();
    }

    public bool GetStepByStepOpt()
    {
        if (!IsCalculatorOptionAllowed)
            return false;

        return Options!.StepByStep;
    }

    public Result SetRawValueOpt(bool value)
    {
        if (!IsCalculatorOptionAllowed)
            return Err("Cannot set calculator option in this scope");

        Options!.Raw = value;
        return Ok();
    }

    public bool GetRawValueOpt()
    {
        if (!IsCalculatorOptionAllowed)
            return false;

        return Options!.Raw;
    }

    public Result SetShowTreeOpt(bool value)
    {
        if (!IsCalculatorOptionAllowed)
            return Err("Cannot set calculator option in this scope");

        Options!.ShowTree = value;
        return Ok();
    }

    public bool GetShowTreeOpt()
    {
        if (!IsCalculatorOptionAllowed)
            return false;

        return Options!.ShowTree;
    }

    public Result<CustomFunctionCollection> GetFnCollection()
    {
        if (!_isCustomFunctionAllowed)
            return Err<CustomFunctionCollection>("Custom function not allowed in this scope");

        return CustomFunctions!;
    }

    public Result AddFn(CustomFunction.CustomFunction fn)
    {
        if (!_isCustomFunctionAllowed)
            return Err<CustomFunctionCollection>("Custom function not allowed in this scope");

        return CustomFunctions!.Add(fn);
    }

    public void EndInit()
    {
        if (_isCustomFunctionAllowed && CustomFunctions!.Count() is not 0)
            CustomFunctions!.End(Variables);
    }

    public static Scope Create(bool isCustomFunctionAllowed = true, bool isCalculatorOptionAllowed = true,
        bool isVariableAllowed = true, bool isCompareAllowed = true)
    {
        var scope = new Scope(isCustomFunctionAllowed, isCalculatorOptionAllowed, isVariableAllowed, isCompareAllowed);

        return scope;
    }
}