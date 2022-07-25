// ReSharper disable CommentTypo

using System.Text;
using Common.Maths.Extension;
using Common.Results;
using LCalc.Helpers;
using LCalc.Helpers.CustomFunction;

namespace LCalc;

/// <summary>
///     A String Calculator
/// </summary>
public static class Calculator
{
    /// <summary>
    ///     Calculate a string. This method shouldn't throw an error
    /// </summary>
    /// <param name="input">Expression</param>
    /// <returns>"Error: {error}" if there is an error. "Result: {result}" or "{steps}\nResult: {result}" otherwise</returns>
    public static string Calc(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "Error: No expression found";

        var splitResult =
            CalculatorHelpers.SplitInput(input.Trim().ToLower(), out var args, out var opts, out var functions);
        if (splitResult.IsFaulted)
            return HandleException(splitResult.Exception!);
        var math = splitResult.Value!;

        InputHandler(ref math, args);

        var compare = TryCompare(math, functions, out var result1);
        if (compare.IsFaulted)
            return HandleException(compare.Exception!);

        string result;
        if (compare.Value)
        {
            result = result1.ToString();
            return $"Result: {result}";
        }

        Result<double> result2;
        if (!opts.StepByStep)
        {
            result2 = Calculate(math, functions);
            if (result2.IsFaulted)
                return HandleException(result2.Exception!);

            if (opts.Raw)
            {
                result = result2.ToString();
                return $"Result: {result}";
            }

            result = result2.Value.ToFraction();
            return $"Result: {result}";
        }

        result2 = Calculate(math, functions, out var stepsString);
        if (result2.IsFaulted)
            return HandleException(result2.Exception!);

        if (opts.Raw)
        {
            result = result2.ToString();
            return $"Result: {result}";
        }

        result = result2.Value.ToFraction();
        return $"{stepsString}\nResult: {result}";
    }

    private static string HandleException(Exception ex)
    {
        return ex switch
        {
            OverflowException => "Error: Value too big",
            IndexOutOfRangeException => "Error: Missing value",
            _ => $"Error: {ex.Message}"
        };
    }

    private static void InputHandler(ref List<CalcElement> math, Dictionary<string, CalcElement> args)
    {
        args.TryAdd("pi", Math.PI);
        args.TryAdd("tau", Math.Tau);
        args.TryAdd("e", Math.E);
        for (var i = 0; i < math.Count; i++)
        {
            var m = math[i];
            if (args.TryGetValue(!m.Contains('.') ? m : m.Substring(0, m.IndexOf('.')), out var val))
                math[i] = val;
        }
    }

    private static Result<bool> TryCompare(List<CalcElement> math, CustomFunctionCollection functions, out bool result)
    {
        result = false;
        var pos = new List<int>();
        for (var i = 0; i < math.Count; i++)
            if (math[i].StringForm is "==" or "!=" or ">=" or "<=" or ">" or "<")
                pos.Add(i);
        if (pos.Count == 0)
            return Ok(false);
        result = true;
        for (var i = 0; i < pos.Count; i++)
        {
            var result1 = false;

            var startPos = i == 0 ? 0 : pos[i - 1] + 1;
            var left = math.GetRange(startPos, pos[i] - startPos);
            var right = math.GetRange(pos[i] + 1,
                pos.Count > i + 1 ? pos[i + 1] - pos[i] - 1 : math.Count - pos[i] - 1);

            var left1 = Calculate(left, functions);
            var right1 = Calculate(right, functions);

            if (!left1.Success)
                return Err<bool>(left1.Exception!);
            if (!right1.Success)
                return Err<bool>(right1.Exception!);

            switch (math[pos[i]].StringForm)
            {
                case "==":
                    result1 = Math.Abs(left1.Value - right1.Value) < 0.0000001;
                    break;
                case "!=":
                    result1 = Math.Abs(left1.Value - right1.Value) > 0.0000001;
                    break;
                case ">=":
                    result1 = left1.Value >= right1.Value;
                    break;
                case "<=":
                    result1 = left1.Value <= right1.Value;
                    break;
                case ">":
                    result1 = left1.Value > right1.Value;
                    break;
                case "<":
                    result1 = left1.Value < right1.Value;
                    break;
            }

            if (result1) continue;
            result = false;
            return Ok(true);
        }

        return Ok(true);
    }

    internal static Result<double> Calculate(List<CalcElement> math, CustomFunctionCollection functions)
    {
        var maxLevel = 0;
        var level = 0;
        foreach (var str in math)
            if (str.EndsWith('('))
            {
                level++;
                if (level > maxLevel)
                    maxLevel = level;
            }
            else if (str.StartsWith(')'))
            {
                level--;
            }

        if (level is not 0)
            return Err<double>("Invalid number of brackets");

        while (maxLevel is not 0)
        {
            level = 0;
            for (var i = 0; i < math.Count; i++)
            {
                if (math[i].StartsWith(')'))
                    level--;
                else if (math[i].EndsWith('('))
                    level++;
                if (level != maxLevel) continue;

                string? opt = null;
                if (math[i].Length is not 1)
                    opt = math[i].Substring(0, math[i].Length - 1);
                var count = math.FindIndex(i + 1, a => a.EndsWith(')')) - i - 1;
                var calculation = math.GetRange(i + 1, count);
                math.RemoveRange(i, count + 1);
                var result = CalcByLevel(calculation, opt, functions);
                if (!result.Success)
                    return Err<double>(result.Exception!);
                math[i].DoubleForm = result.Value;

                level--;
            }

            maxLevel--;
        }

        return CalcByLevel(math, null, functions);
    }

    private static Result<double> Calculate(List<CalcElement> math, CustomFunctionCollection functions,
        out string stepsString)
    {
        var maxLevel = 0;
        var level = 0;
        foreach (var str in math)
            if (str.EndsWith('('))
            {
                level++;
                if (level > maxLevel)
                    maxLevel = level;
            }
            else if (str.StartsWith(')'))
            {
                level--;
            }

        if (level is not 0)
        {
            stepsString = string.Empty;
            return Err<double>("Invalid number of brackets");
        }

        var strBuilder = new StringBuilder();
        while (maxLevel is not 0)
        {
            level = 0;
            for (var i = 0; i < math.Count; i++)
            {
                if (math[i].StartsWith(')'))
                    level--;
                else if (math[i].EndsWith('('))
                    level++;
                if (level != maxLevel) continue;

                string? opt = null;
                if (math[i].Length is not 1)
                    opt = math[i].Substring(0, math[i].Length - 1);
                var count = math.FindIndex(i + 1, a => a.EndsWith(')')) - i - 1;
                var calculation = math.GetRange(i + 1, count);
                math.RemoveRange(i, count + 1);
                var result = CalcByLevel(calculation, opt, functions);
                if (!result.Success)
                {
                    stepsString = string.Empty;
                    return Err<double>(result.Exception!);
                }

                math[i].DoubleForm = result.Value;
                strBuilder.Append(string.Join(' ', math));
                strBuilder.Append('\n');

                level--;
            }

            maxLevel--;
        }

        stepsString = strBuilder.ToString();
        return CalcByLevel(math, null, functions);
    }

    private static Result<double> CalcByLevel(List<CalcElement> math, string? opt, CustomFunctionCollection functions)
    {
        // if math contains only one element and it is double, return it
        if (math.Count is 1 && opt is null && math[0].IsNumber)
            return math[0].GetValue();

        var result = CalculatorHelpers.CalcExponent(math);
        if (result.IsFaulted)
            return result;

        result = CalculatorHelpers.CalcFactorial(math);
        if (result.IsFaulted)
            return result;

        result = CalculatorHelpers.CalcBitwise(math);
        if (result.IsFaulted)
            return result;

        switch (opt)
        {
            case null:
                break;
            case "random":
                result = CalculatorHelpers.CalcRandom(math);
                break;
            case "gcd":
                result = CalculatorHelpers.CalcGcd(math);
                break;
            case "lcm":
                result = CalculatorHelpers.CalcLcm(math);
                break;
            case "sin":
                result = CalculatorHelpers.CalcSin(math);
                break;
            case "cos":
                result = CalculatorHelpers.CalcCos(math);
                break;
            case "tan":
                result = CalculatorHelpers.CalcTan(math);
                break;
            case "cot":
                result = CalculatorHelpers.CalcCot(math);
                break;
            case "sqrt":
                result = CalculatorHelpers.CalcSqrt(math);
                break;
            case "cbrt":
                result = CalculatorHelpers.CalcCbrt(math);
                break;
            case "abs":
                result = CalculatorHelpers.CalcAbs(math);
                break;
            case "log":
                result = CalculatorHelpers.CalcLog(math);
                break;
            case "floor":
                result = CalculatorHelpers.CalcFloor(math);
                break;
            case "ceiling":
                result = CalculatorHelpers.CalcCeiling(math);
                break;
            case "round":
                result = CalculatorHelpers.CalcRound(math);
                break;
            case "avg":
                result = CalculatorHelpers.CalcAvg(math);
                break;
            case "sum":
                result = CalculatorHelpers.CalcSum(math);
                break;
            default:
                result = functions.Execute(opt, math);
                break;
        }

        if (result.IsFaulted)
            return result;

        result = CalculatorHelpers.CalcNormal(math);
        if (result.IsFaulted)
            return result;

        if (math.Count is not 1)
            return Err<double>("Missing operator");

        return math[0].GetValue();
    }
}