using Common.Results;
using LCalc.CustomFunction;
using LCalc.Variables;

namespace LCalc;

internal sealed class Scope
{
    public Scope() : this(true, true, true, true)
    {
    }

    public Scope(bool all) : this(all, all, all, all)
    {
    }

    public Scope(bool isCustomFunctionAllowed, bool isCalculatorOptionAllowed, bool isVariableAllowed,
        bool isCompareAllowed)
    {
        Option = CalculatorOption.None;

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

    public Scope(CalculatorOption options)
    {
        Option = SanitizeOptions(options);

        Variables = new VariableCollection();
        CustomFunctions = new CustomFunctionCollection();
    }

    public CalculatorOption Option { get; set; }
    public CustomFunctionCollection? CustomFunctions { get; set; }
    public IVariableCollection Variables { get; set; }
    public Format Format { get; set; } = Format.Human;

    public bool IsCustomFunctionAllowed => CustomFunctions is not null;
    public bool IsCompareAllowed => (Option & CalculatorOption.CompareAllowed) != 0;
    public bool IsCalculatorOptionAllowed => (Option & CalculatorOption.CalculatorOptionAllowed) != 0;
    public bool IsVariableAllowed => (Option & CalculatorOption.VariableAllowed) != 0;

    public void FinalizeOption()
    {
        if ((Option & CalculatorOption.LaTeX) != 0 && (Option & CalculatorOption.Render) == 0)
        {
            Option |= CalculatorOption.Step;
        }
    }

    private static CalculatorOption SanitizeOptions(CalculatorOption options)
    {
        if ((options & CalculatorOption.Tree) != 0)
            options |= CalculatorOption.Step;

        if ((options & CalculatorOption.LaTeXDoc) != 0)
            options |= CalculatorOption.Step | CalculatorOption.LaTeX;
        
        return options;
    }

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
        if (Variables.TryAdd(name, value))
            return Ok();

        return Err($"Variable '{name}' had already been set");
    }

    public void SetOpt(CalculatorOption op)
    {
        Option |= op;
    }

    public bool GetStepByStepOpt()
    {
        return (Option & CalculatorOption.Step) != 0;
    }

    public bool GetShowTreeOpt()
    {
        return (Option & CalculatorOption.Tree) != 0;
    }

    public bool GetSolveOpt()
    {
        return (Option & CalculatorOption.Solve) != 0;
    }

    public bool GetLaTeXOpt()
    {
        return (Option & CalculatorOption.LaTeX) != 0;
    }

    public bool GetLaTeXDocOpt()
    {
        return (Option & CalculatorOption.LaTeXDoc) != 0;
    }

    public bool GetNoLaTeXDocOpt()
    {
        return (Option & CalculatorOption.NoLaTeXDoc) != 0;
    }

    public bool GetRenderOpt()
    {
        return (Option & CalculatorOption.Render) != 0;
    }

    public void EndInit()
    {
        if (CustomFunctions is null)
            return;

        CustomFunctions.End(Variables, Option);
    }
}