using Common.Results;
using LCalc.CustomFunction;

namespace LCalc;

internal sealed class Scope
{
    public Scope() : this(true, true, true, true)
    {
    }

    public Scope(bool isCustomFunctionAllowed, bool isCalculatorOptionAllowed, bool isVariableAllowed,
        bool isCompareAllowed)
    {
        Option = 0;

        if (isVariableAllowed)
            Option = CalculatorOption.VariableAllowed;

        if (isCalculatorOptionAllowed)
            Option |= CalculatorOption.CalculatorOptionAllowed;

        if (isCompareAllowed)
            Option |= CalculatorOption.CompareAllowed;

        if (isCustomFunctionAllowed)
            CustomFunctions = new CustomFunctionCollection();
        Variables = new VariableCollection();
    }

    public CalculatorOption Option { get; set; }
    public CustomFunctionCollection? CustomFunctions { get; set; }
    public IVariableCollection Variables { get; set; }

    public bool CustomFunctionAllowed => CustomFunctions is not null;
    public bool IsCompareAllowed => (Option & CalculatorOption.CompareAllowed) != 0;
    public bool IsCalculatorOptionAllowed => (Option & CalculatorOption.CalculatorOptionAllowed) != 0;

    public Result<double> GetVariable(string name)
    {
        if (Variables.TryGet(name, out var variable)) // Allow for some standard variable like: pi
            return variable;

        if ((Option & CalculatorOption.VariableAllowed) == 0)
            return Err<double>("Variable not allowed in this scope");

        return Err<double>($"Unknown variable '{name}'");
    }

    public Result SetVariable(string name, double value)
    {
        if ((Option & CalculatorOption.VariableAllowed) == 0)
            return Err("Variable not allowed in this scope");

        if (Variables.TryAdd(name, value))
            return Ok();

        return Err($"Variable '{name}' had already been set");
    }

    public Result SetStepByStepOpt()
    {
        if (!IsCalculatorOptionAllowed)
            return Err("Cannot set calculator option in this scope");

        Option |= CalculatorOption.StepByStep;
        return Ok();
    }

    public bool GetStepByStepOpt()
    {
        return (Option & CalculatorOption.StepByStep) != 0;
    }

    public Result SetRawValueOpt()
    {
        if (!IsCalculatorOptionAllowed)
            return Err("Cannot set calculator option in this scope");

        Option |= CalculatorOption.Raw;
        return Ok();
    }

    public bool GetRawValueOpt()
    {
        return (Option & CalculatorOption.Raw) != 0;
    }

    public Result SetShowTreeOpt()
    {
        if (!IsCalculatorOptionAllowed)
            return Err("Cannot set calculator option in this scope");

        Option |= CalculatorOption.ShowTree;
        return Ok();
    }

    public bool GetShowTreeOpt()
    {
        return (Option & CalculatorOption.ShowTree) != 0;
    }

    public Result SetSolveOpt()
    {
        if (!IsCalculatorOptionAllowed)
            return Err("Cannot set calculator option in this scope");

        Option |= CalculatorOption.Solve;
        return Ok();
    }

    public bool GetSolveOpt()
    {
        return (Option & CalculatorOption.Solve) != 0;
    }

    public Result<CustomFunctionCollection> GetFnCollection()
    {
        if (CustomFunctions is null)
            return Err<CustomFunctionCollection>("Custom function not allowed in this scope");

        return CustomFunctions!;
    }

    public Result AddFn(CustomFunction.CustomFunction fn)
    {
        if (CustomFunctions is null)
            return Err<CustomFunctionCollection>("Custom function not allowed in this scope");

        return CustomFunctions!.Add(fn);
    }

    public void EndInit()
    {
        if (CustomFunctions is null)
            return;

        CustomFunctions.End(Variables, Option);
    }
}