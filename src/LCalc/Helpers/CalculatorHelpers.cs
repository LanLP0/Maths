using Common.Maths;
using Common.Results;
using LCalc.MathTree;
using LCalc.MathTree.Nodes;
using LCalc.Variables;

namespace LCalc.Helpers;

internal static class CalculatorHelpers
{
    private static readonly Random Rng = new();

    //
    // Special functions
    //

    public static Result<double> CalcSigma(List<IMathNode> maths, Scope scope)
    {
        if (maths.Count is not 4)
            return Err("sigma() takes exactly 4 arguments");

        if (maths[0] is not VariableNode variable)
            return Err("sigma(): First argument must be a variable");

        var start = maths[1].Calc(scope);
        if (start.Faulted)
            return start;

        var startVal = start.Value;
        if (!startVal.IsInt())
            return Err("sigma(): Start must be an integer");

        var end = maths[2].Calc(scope);
        if (end.Faulted)
            return end;

        var endVal = end.Value;
        if (!endVal.IsInt())
            return Err("sigma(): End must be an integer");

        if (endVal < startVal)
            return Err("sigma(): End cannot be less than start");

        var fn = maths[3];

        // Setup variable
        var ogVarCollection = scope.Variables;
        var variable1 = new Variable(variable.Name, 0);
        scope.Variables = new SingleVariableCollection(variable1, ogVarCollection);

        var setupResult = fn.SetupForSolving(scope, out var unknown);
        if (setupResult.Faulted)
            return setupResult;

        if (unknown != variable.Name)
            return Err("sigma(): Invalid variable name");

        var result = 0.0;
        for (; startVal <= endVal; startVal++)
        {
            variable1.Value = startVal;

            var runResult = fn.Calc(scope);
            if (runResult.Faulted)
                return runResult;

            result += runResult.Value;
        }

        scope.Variables = ogVarCollection;
        return result;
    }

    public static Result<double> CalcCPi(List<IMathNode> maths, Scope scope)
    {
        if (maths.Count is not 4)
            return Err("cpi() takes exactly 4 arguments");

        if (maths[0] is not VariableNode variable)
            return Err("cpi(): First argument must be a variable");

        var start = maths[1].Calc(scope);
        if (start.Faulted)
            return start;

        var startVal = start.Value;
        if (!startVal.IsInt())
            return Err("cpi(): Start must be an integer");

        var end = maths[2].Calc(scope);
        if (end.Faulted)
            return end;

        var endVal = end.Value;
        if (!endVal.IsInt())
            return Err("cpi(): End must be an integer");

        if (endVal < startVal)
            return Err("cpi(): End cannot be less than start");

        var fn = maths[3];

        // Setup variable
        var ogVarCollection = scope.Variables;
        var variable1 = new Variable(variable.Name, 0);
        scope.Variables = new SingleVariableCollection(variable1, ogVarCollection);

        var setupResult = fn.SetupForSolving(scope, out var unknown);
        if (setupResult.Faulted)
            return setupResult;

        if (unknown != variable.Name)
            return Err("cpi(): Invalid variable name");

        var result = 1.0;
        for (; startVal <= endVal; startVal++)
        {
            variable1.Value = startVal;

            var runResult = fn.Calc(scope);
            if (runResult.Faulted)
                return runResult;

            result *= runResult.Value;
        }

        scope.Variables = ogVarCollection;
        return result;
    }

    //
    // Functions
    //

    public static Result<double> CalcCbrt(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not 1)
            return Err("cbrt() accept exactly 1 argument");

        return Math.Cbrt(math[0]);
    }

    public static Result<double> CalcSqrt(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not 1)
            return Err("sqrt() accept exactly 1 argument");

        return Math.Sqrt(math[0]);
    }

    public static Result<double> CalcCeiling(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not 1)
            return Err("ceiling() accept exactly 1 argument");

        return Math.Ceiling(math[0]);
    }

    public static Result<double> CalcRound(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not (1 or 2))
            return Err("round() accept exactly 1 - 2 argument");

        var num = math[0];

        if (math.Length is not 2)
            return Math.Round(num);

        var asIntResult = math[1].ToInt();
        if (asIntResult.Faulted)
            return asIntResult.Exception!;

        var digits = asIntResult.Value;
        if (digits is > 5 or < 0)
            return Err("digits must be between 0 - 5");

        return Math.Round(num, digits);
    }

    public static Result<double> CalcFloor(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not 1)
            return Err("floor() accept exactly 1 argument");

        return Math.Floor(math[0]);
    }

    public static Result<double> CalcAbs(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not 1)
            return Err("abs() accept exactly 1 argument");

        return Math.Abs(math[0]);
    }

    public static Result<double> CalcSin(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not 1)
            return Err("sin() takes exactly one argument");

        return Math.Sin(math[0].ToRadians());
    }

    public static Result<double> CalcCos(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not 1)
            return Err("cos() takes exactly one argument");

        return Math.Cos(math[0].ToRadians());
    }

    public static Result<double> CalcTan(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not 1)
            return Err("tan() takes exactly one argument");

        return Math.Tan(math[0].ToRadians());
    }

    public static Result<double> CalcCot(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not 1)
            return Err("cot() takes exactly one argument");

        return 1 / Math.Tan(math[0].ToRadians());
    }

    public static Result<double> CalcLog(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is not 1)
            return Err("log() takes exactly one argument");

        return Math.Log(math[0]);
    }

    public static Result<double> CalcRandom(scoped ReadOnlySpan<double> math)
    {
        if (math.Length > 2)
            return Err("random() takes at most 2 arguments");

        var max = 1.0;
        var min = 0.0;
        if (math.Length is not 0)
        {
            max = math[0];
            min = 0;

            if (math.Length is not 1)
            {
                min = max;
                max = math[1];
            }
        }

        return Rng.NextDouble() * (max - min) + min;
    }

    public static Result<double> CalcGcd(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is 0)
            return Err("gcd() require at least 1 argument");

        if (!math[0].IsInt())
            return Err("gcd() only accept integers");

        var num1 = math[0];
        for (var i = 1; i < math.Length; i++)
        {
            var num2 = math[i];
            if (!num2.IsInt())
                return Err("gcd() only accept integers");

            num1 = Maths.FastGcd(num1, num2);
        }

        return num1;
    }

    public static Result<double> CalcLcm(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is 0)
            return Err("lcm() require at least 1 argument");

        if (!math[0].IsInt())
            return Err("lcm() only accept integers");

        var num1 = math[0];
        for (var i = 1; i < math.Length; i++)
        {
            var num2 = math[i];
            if (!num2.IsInt())
                return Err("lcm() only accept integers");

            num1 = Maths.FastLcm(num1, num2);
        }

        return num1;
    }

    public static Result<double> CalcAvg(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is 0)
            return Err("avg() takes at least one argument");

        double total = 0;
        var numberOfArgs = 0;
        foreach (var e in math)
        {
            total += e;
            numberOfArgs++;
        }

        return total / numberOfArgs;
    }

    public static Result<double> CalcSum(scoped ReadOnlySpan<double> math)
    {
        if (math.Length is 0)
            return Err("sum() takes at least one argument");

        double total = 0;
        foreach (var e in math) total += e;

        return total;
    }

    //
    // Misc
    //

    public static Result<double> ParseNumber(ReadOnlySpan<char> value)
    {
        var e = 0.0;

        if (value[value.Length - 1] == '%')
        {
            if (!double.TryParse(value.Slice(0, value.Length - 1), out var result))
                return Err<double>($"{value.ToString()} is not a number");

            e = result / 100;
            return e;
        }

        if (value[0] == '-')
        {
            if (value.Length is 1)
                return Err("- is not a number");

            if (value[1] != '0')
            {
                if (!double.TryParse(value, out var result))
                    return Err<double>($"{value.ToString()} is not a number");

                e = result;
                return e;
            }

            if (value.Length is 2)
            {
                if (!double.TryParse(value, out var result))
                    return Err($"{value.ToString()} is not a number");

                e = result;
                return e;
            }

            switch (value[2])
            {
                case 'x': // hex num
                {
                    if (value.Length < 3)
                        return Err($"{value.ToString()} is not a number");

                    var result = HexStringToNumber(value.Slice(3));
                    if (result.Faulted)
                        return Err<double>(result.Exception!);

                    e = -result.Value;
                    break;
                }
                case 'b': // binary num
                {
                    if (value.Length < 3)
                        return Err($"{value.ToString()} is not a number");

                    var result = BinaryStringToNumber(value.Slice(3));
                    if (result.Faulted)
                        return Err<double>(result.Exception!);

                    e = -result.Value;
                    break;
                }
                case 'o': // octal num
                {
                    if (value.Length < 3)
                        return Err($"{value.ToString()} is not a number");

                    var result = OctalStringToNumber(value.Slice(3));
                    if (result.Faulted)
                        return Err<double>(result.Exception!);

                    e = -result.Value;
                    break;
                }
                default:
                {
                    if (!double.TryParse(value, out var result))
                        return Err($"{value.ToString()} is not a number");

                    e = result;
                    break;
                }
            }

            return e;
        }

        if (value[0] != '0')
        {
            if (!double.TryParse(value, out var result))
                return Err<double>($"{value.ToString()} is not a number");

            e = result;
            return e;
        }

        if (value.Length is 1)
            return e;

        switch (value[1])
        {
            case 'x': // hex num
            {
                if (value.Length < 3)
                    return Err($"{value.ToString()} is not a number");

                var result = HexStringToNumber(value.Slice(2));
                if (result.Faulted)
                    return Err<double>(result.Exception!);

                e = result.Value;
                break;
            }
            case 'b': // binary num
            {
                if (value.Length < 3)
                    return Err($"{value.ToString()} is not a number");

                var result = BinaryStringToNumber(value.Slice(2));
                if (result.Faulted)
                    return Err<double>(result.Exception!);

                e = result.Value;
                break;
            }
            case 'o': // octal num
            {
                if (value.Length < 3)
                    return Err($"{value.ToString()} is not a number");

                var result = OctalStringToNumber(value.Slice(2));
                if (result.Faulted)
                    return Err<double>(result.Exception!);

                e = result.Value;
                break;
            }
            default:
            {
                if (!double.TryParse(value, out var result))
                    return Err($"{value.ToString()} is not a number");

                e = result;
                break;
            }
        }

        return e;
    }

    public static Result<double> OctalStringToNumber(ReadOnlySpan<char> s)
    {
        var result = 0.0;
        const long radix = 8;

        foreach (var chr in s)
        {
            if ((int)chr is not (> 47 and < 56))
                return Err<double>("Invalid octal number");

            result = result * radix + (chr - 48);
        }

        return result;
    }

    public static Result<double> HexStringToNumber(ReadOnlySpan<char> s)
    {
        var result = 0.0;
        const long radix = 16;

        foreach (var chr in s)
        {
            int val;
            
            switch ((int)chr)
            {
                case 97:  // a
                case 98:  // b
                case 99:  // c
                case 100: // d
                case 101: // e
                case 102: // f
                    val = chr - 87;
                    break;
                case 48: // 0
                case 49: // 1
                case 50: // 2
                case 51: // 3
                case 52: // 4
                case 53: // 5
                case 54: // 6
                case 55: // 7
                case 56: // 8
                case 57: // 9
                    val = chr - 48;
                    break;
                default:
                    return Err<double>("Invalid hex number");
            }

            result = result * radix + val;
        }

        return result;
    }

    public static Result<double> BinaryStringToNumber(ReadOnlySpan<char> s)
    {
        var result = 0.0;
        const long radix = 2;

        foreach (var chr in s)
        {
            if ((int)chr is not (48 or 49))
                return Err<double>("Invalid binary number");

            result = result * radix + (chr - 48);
        }

        return result;
    }
}