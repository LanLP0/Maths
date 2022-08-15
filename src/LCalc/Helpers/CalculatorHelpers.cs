using Common.Maths;
using Common.Maths.Extension;
using Common.Results;
using LCalc.CustomFunction;
using LCalc.Extension;

namespace LCalc.Helpers;

internal static class CalculatorHelpers
{
    private static readonly Random Rng = new();

    public static Result CalcBitwise(List<CalcElement> math)
    {
        for (var i = 0; i < math.Count; i++)
        {
            var el = math[i];
            if (!el.IsString)
                continue;

            switch (el.StringForm)
            {
                case "&":
                {
                    var check = Guard.IndexInRange(math, i - 1, i + 1);
                    if (check.IsFaulted)
                        return check;

                    var a = math[i - 1].AsInt64();
                    if (a.IsFaulted) return a;

                    var b = math[i + 1].AsInt64();
                    if (b.IsFaulted) return b;

                    math.RemoveRange(i - 1, 2);
                    var e = math[i - 1];
                    e.DoubleForm = a.Value & b.Value;
                    i--;
                    break;
                }
                case "|":
                {
                    var check = Guard.IndexInRange(math, i - 1, i + 1);
                    if (check.IsFaulted)
                        return check;

                    var a = math[i - 1].AsInt64();
                    if (a.IsFaulted) return a;

                    var b = math[i + 1].AsInt64();
                    if (b.IsFaulted) return b;

                    math.RemoveRange(i - 1, 2);
                    var e = math[i - 1];
                    e.DoubleForm = a.Value | b.Value;
                    i--;
                    break;
                }
                case "~":
                {
                    var check = Guard.IndexInRange(math, i + 1);
                    if (check.IsFaulted)
                        return check;

                    var a = math[i + 1];
                    var val = a.AsInt64();
                    if (val.IsFaulted) return val;

                    a.DoubleForm = ~val.Value;
                    math.RemoveAt(i);
                    i--;
                    break;
                }
                case "<<":
                {
                    var check = Guard.IndexInRange(math, i - 1, i + 1);
                    if (check.IsFaulted)
                        return check;

                    var a = math[i - 1].AsInt();
                    if (a.IsFaulted) return a;

                    var b = math[i + 1].AsInt();
                    if (b.IsFaulted) return b;

                    math.RemoveRange(i - 1, 2);
                    var e = math[i - 1];
                    e.DoubleForm = a.Value << b.Value;
                    i--;
                    break;
                }
                case ">>":
                {
                    var check = Guard.IndexInRange(math, i - 1, i + 1);
                    if (check.IsFaulted)
                        return check;

                    var a = math[i - 1].AsInt();
                    if (a.IsFaulted) return a;

                    var b = math[i + 1].AsInt();
                    if (b.IsFaulted) return b;

                    math.RemoveRange(i - 1, 2);
                    var e = math[i - 1];
                    e.DoubleForm = a.Value >> b.Value;
                    i--;
                    break;
                }
                case "^^":
                {
                    var check = Guard.IndexInRange(math, i - 1, i + 1);
                    if (check.IsFaulted)
                        return check;

                    var a = math[i - 1].AsInt64();
                    if (a.IsFaulted) return a;

                    var b = math[i + 1].AsInt64();
                    if (b.IsFaulted) return b;

                    math.RemoveRange(i - 1, 2);
                    var e = math[i - 1];
                    e.DoubleForm = a.Value ^ b.Value;
                    i--;
                    break;
                }
            }
        }

        return Ok();
    }

    public static Result CalcExponent(List<CalcElement> math)
    {
        // var i = math.Count - 1;
        for (var i = math.Count - 1; i >= 0; i--)
        {
            // i = math.FindLastIndex(i, a => !a.IsString && a.StringForm is "^");
            // if (i is -1)
            //     return Ok();
            var e = math[i];
            if (!(e.IsString && e.StringForm.Length is 1 && e.StringForm[0] is '^'))
                continue;

            var check = Guard.IndexInRange(math, i - 1, i + 1);
            if (check.IsFaulted)
                return check;

            var left = math[i - 1].GetValue();
            if (left.IsFaulted)
                return left;

            var right = math[i + 1];
            var rightNum = right.GetValue();
            if (rightNum.IsFaulted)
                return rightNum;

            right.DoubleForm = Math.Pow(left, rightNum);
            math.RemoveRange(i - 1, 2);
            i--;
        }

        return Ok();
    }

    public static Result CalcFactorial(List<CalcElement> math)
    {
        // var index = 0;
        for (var i = 0; i < math.Count; i++)
        {
            // index = math.FindIndex(index, a => a.IsString && a.StringForm is "!");
            var e = math[i];
            // if (index is -1)
            //     return Ok();
            if (!(e.IsString && e.StringForm.Length is 1 && e.StringForm[0] is '!'))
                continue;

            var left = math[i - 1];
            if (!left.IsInt)
                return Err($"{left.StringForm} is not an integer");

            var val = left.RiskyGetValue();

            for (var o = 1; o < val; o++)
                left.DoubleForm = left.RiskyGetValue() * o;

            math.RemoveAt(i);
        }

        return Ok();
    }

    public static Result CalcCbrt(List<CalcElement> math)
    {
        if (math.Count is not 1)
            return Err("cbrt() accept exactly 1 argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        e.DoubleForm = Math.Cbrt(val.Value);

        return Ok();
    }

    public static Result CalcSqrt(List<CalcElement> math)
    {
        if (math.Count is not 1)
            return Err("sqrt() accept exactly 1 argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        e.DoubleForm = Math.Sqrt(val.Value);

        return Ok();
    }

    public static Result CalcCeiling(List<CalcElement> math)
    {
        if (math.Count is not 1)
            return Err("ceiling() accept exactly 1 argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        e.DoubleForm = Math.Ceiling(val.Value);

        return Ok();
    }

    public static Result CalcRound(List<CalcElement> math)
    {
        if (math.Count is not (1 or 2))
            return Err("round() accept exactly 1 - 2 argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        var digits = 0;
        if (math.Count is 2)
        {
            var asIntResult = math[1].AsInt();
            if (asIntResult.IsFaulted) return asIntResult;

            digits = asIntResult.Value;
            if (digits is > 15 or < 0) return Err("digits must be between 0 - 15");

            math.RemoveAt(1);
        }

        e.DoubleForm = Math.Round(val.Value, digits);

        return Ok();
    }

    public static Result CalcFloor(List<CalcElement> math)
    {
        if (math.Count is not 1)
            return Err("floor() accept exactly 1 argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        e.DoubleForm = Math.Floor(val.Value);

        return Ok();
    }

    public static Result CalcAbs(List<CalcElement> math)
    {
        if (math.Count is not 1)
            return Err("abs() accept exactly 1 argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        e.DoubleForm = Math.Abs(val.Value);

        return Ok();
    }

    public static Result CalcSin(List<CalcElement> math)
    {
        if (math.Count is not 1)
            return Err("sin() takes exactly one argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        e.DoubleForm = Math.Sin(val.Value.ToRadians());

        return Ok();
    }

    public static Result CalcCos(List<CalcElement> math)
    {
        if (math.Count is not 1)
            return Err("cos() takes exactly one argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        e.DoubleForm = Math.Cos(val.Value.ToRadians());

        return Ok();
    }

    public static Result CalcTan(List<CalcElement> math)
    {
        if (math.Count is not 1)
            return Err("tan() takes exactly one argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        e.DoubleForm = Math.Tan(val.Value.ToRadians());

        return Ok();
    }

    public static Result CalcCot(List<CalcElement> math)
    {
        if (math.Count is not 1)
            return Err("cot() takes exactly one argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        e.DoubleForm = 1 / Math.Tan(val.Value.ToRadians());

        return Ok();
    }

    public static Result CalcLog(List<CalcElement> math)
    {
        if (math.Count is not 1)
            return Err("log() takes exactly one argument");

        var e = math[0];
        var val = e.GetValue();
        if (val.IsFaulted) return val;

        e.DoubleForm = Math.Log(val.Value);

        return Ok();
    }

    public static Result CalcRandom(List<CalcElement> math)
    {
        if (math.Count > 2)
            return Err("random() takes at most 2 arguments");

        double result;
        if (math.Count is not 0)
        {
            var max = math[0];
            var maxVal = max.GetValue();
            if (maxVal.IsFaulted) return maxVal;

            double min = 0;
            if (math.Count is not 1)
            {
                min = maxVal;
                maxVal = math[1].GetValue();
                if (maxVal.IsFaulted) return maxVal;
            }

            result = Rng.NextDouble() * (maxVal - min) + min;
        }
        else
        {
            result = Rng.NextDouble();
        }

        math.Clear();
        math.Add(result);

        return Ok();
    }

    public static Result CalcGcd(List<CalcElement> math)
    {
        if (math.Count is 0)
            return Err("gcd() require at least 1 argument");

        for (var i = math.Count - 1; i > 0; i--)
        {
            var e = math[i - 1];
            var val1 = e.AsInt64();
            if (val1.IsFaulted) return val1;

            var val2 = math[i].AsInt64();
            if (val2.IsFaulted) return val2;

            e.DoubleForm = Maths.GetGcd(val1.Value, val2.Value);
            math.RemoveAt(i);
        }

        return Ok();
    }

    public static Result CalcLcm(List<CalcElement> math)
    {
        if (math.Count is 0)
            return Err("lcm() require at least 1 argument");

        for (var i = math.Count - 1; i > 0; i--)
        {
            var e = math[i - 1];
            var val1 = e.AsInt64();
            if (val1.IsFaulted) return val1;

            var val2 = math[i].AsInt64();
            if (val2.IsFaulted) return val2;

            e.DoubleForm = Maths.GetLcm(val1.Value, val2.Value);
            math.RemoveAt(i);
        }

        return Ok();
    }

    public static Result CalcAvg(List<CalcElement> math)
    {
        if (math.Count is 0)
            return Err("avg() takes at least one argument");

        var e = math[0];
        double total = 0;
        foreach (var e1 in math)
        {
            var val = e1.GetValue();
            if (val.IsFaulted) return val;

            total += val;
        }

        e.DoubleForm = total / math.Count;
        if (math.Count is not 1)
            math.RemoveRange(1, math.Count - 1);

        return Ok();
    }

    public static Result CalcSum(List<CalcElement> math)
    {
        if (math.Count is 0)
            return Err("sum() takes at least one argument");

        var e = math[0];
        double total = 0;
        foreach (var e1 in math)
        {
            var val = e1.GetValue();
            if (val.IsFaulted) return val;

            total += val;
        }

        e.DoubleForm = total;
        if (math.Count is not 1)
            math.RemoveRange(1, math.Count - 1);

        return Ok();
    }

    public static Result CalcNormal(List<CalcElement> math)
    {
        var result = Mod(math);
        if (!result.Success)
            return result;

        result = MulDiv(math);
        if (!result.Success)
            return result;

        result = PlusMinus(math);
        return result;
    }

    private static Result Mod(List<CalcElement> math)
    {
        var i = 0;
        for (;;)
        {
            i = math.FindIndex(i, a => a.StartsWith('%'));
            if (i is -1)
                return Ok();

            var check = Guard.IndexInRange(math, i - 1, i + 1);
            if (check.IsFaulted)
                return check;

            var left = math[i - 1];
            var leftVal = left.GetValue();
            if (leftVal.IsFaulted) return leftVal;

            var right = math[i + 1];
            var rightVal = right.GetValue();
            if (rightVal.IsFaulted) return rightVal;

            left.DoubleForm = leftVal % rightVal;
            math.RemoveRange(i, 2);
            i--;
        }
    }

    private static Result MulDiv(List<CalcElement> math)
    {
        for (var i = 0; i < math.Count; i++)
        {
            var op = math[i];
            
            if (op.StartsWith('*'))
            {
                var check = Guard.IndexInRange(math, i - 1, i + 1);
                if (check.IsFaulted)
                    return check;

                var left = math[i - 1];
                var leftVal = left.GetValue();
                if (leftVal.IsFaulted) return leftVal;

                var right = math[i + 1];
                var rightVal = right.GetValue();
                if (rightVal.IsFaulted) return rightVal;

                left.DoubleForm = leftVal * rightVal;
                math.RemoveRange(i, 2);
                i--;
                continue;
            }

            if (!op.StartsWith('/')) continue;
            {
                var check = Guard.IndexInRange(math, i - 1, i + 1);
                if (check.IsFaulted)
                    return check;

                var left = math[i - 1];
                var leftVal = left.GetValue();
                if (leftVal.IsFaulted) return leftVal;

                var right = math[i + 1];
                var rightVal = right.GetValue();
                if (rightVal.IsFaulted) return rightVal;

                left.DoubleForm = leftVal / rightVal;
                math.RemoveRange(i, 2);
                i--;
            }
        }

        return Ok();
    }

    private static Result PlusMinus(List<CalcElement> math)
    {
        for (var i = 0; i < math.Count; i++)
        {
            var op = math[i];
            if (!op.IsString && op.Length is not 1) continue;
            
            if (op.StartsWith('+'))
            {
                var check = Guard.IndexInRange(math, i - 1, i + 1);
                if (check.IsFaulted)
                    return check;

                var left = math[i - 1];
                var leftVal = left.GetValue();
                if (leftVal.IsFaulted) return leftVal;

                var right = math[i + 1];
                var rightVal = right.GetValue();
                if (rightVal.IsFaulted) return rightVal;

                left.DoubleForm = leftVal + rightVal;
                math.RemoveRange(i, 2);
                i--;
                continue;
            }

            if (!op.StartsWith('-')) continue;
            {
                var check = Guard.IndexInRange(math, i + 1);
                if (check.IsFaulted) return check;

                var right = math[i + 1];
                var rightVal = right.GetValue();
                if (rightVal.IsFaulted) return rightVal;

                if (i - 1 < 0)
                {
                    op.DoubleForm = -rightVal.Value;
                    math.RemoveAt(i + 1);
                    continue;
                }

                var left = math[i - 1];
                var leftVal = left.GetValue();
                if (leftVal.IsFaulted) return leftVal;

                left.DoubleForm = leftVal - rightVal;
                math.RemoveRange(i, 2);
                i--;
            }
        }

        return Ok();
    }

    public static Result<List<CalcElement>> SplitInput(string math, out Dictionary<string, CalcElement> args,
        out CalcOptions opts, out CustomFunctionCollection functions)
    {
        args = new Dictionary<string, CalcElement>();
        opts = new CalcOptions();
        functions = new CustomFunctionCollection();
        var result = new List<CalcElement>();

        /* Item can be in buffer:
         * Number (Ex: -123.56, 69)
         * Arg (Ex: abbhdsh, abdsjgad)
         * Other arg (Ex: &step, &shdjagdjas=12136913)
         * Hex: 0x{value}
         * Binary: 0b{value}
         * Octal: 0o{value}
         */
        var buffer = new InputBuffer(result, args);

        Result result1;
        for (var i = 0; i < math.Length; i++)
        {
            var chr = math[i];

            switch ((int)chr)
            {
                case 43: // +
                case 42: // *
                case 47: // /
                case 37: // %
                case 124: // |
                {
                    if (i - 1 < 0)
                        return Err<List<CalcElement>>($"No value before {chr}");

                    if (i + 1 >= math.Length)
                        return Err<List<CalcElement>>($"No value after {chr}");

                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    result.Add(chr);
                    break;
                }
                case 45: // -
                {
                    if (!math.TryGetValueAt(i - 1, out var prevChr))
                    {
                        buffer.Append('-', BufferContentType.Number);
                        break;
                    }

                    if ((int)prevChr is not (> 47 and < 58)/*0-9*/ and not (> 96 and < 123) /*a-z*/ and not (41/*)*/ or 61/*=*/))
                    {
                        buffer.Append('-', BufferContentType.Number);
                        break;
                    }

                    if (!math.TryGetValueAt(i + 1, out var nextChr))
                        return Err<List<CalcElement>>("No value after -");

                    if (buffer.Content is BufferContentType.Variable)
                    {
                        buffer.Append('-');
                        break;
                    }

                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    if ((int)nextChr is > 47 and < 58/*0-9*/ &&
                        (int)prevChr is not (> 47 and < 58)/*0-9*/ and not (> 96 and < 123)/*a-z*/ and not 41/*)*/)
                    {
                        buffer.Append('-', BufferContentType.Number);
                        break;
                    }

                    result.Add("-");
                    break;
                }
                case 126: // ~
                {
                    if (i + 1 >= math.Length)
                        return Err<List<CalcElement>>($"No value after {chr}");

                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    result.Add(chr);
                    break;
                }
                case 41: // )
                {
                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    result.Add(")");
                    break;
                }
                case 32: // ' '
                {
                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    break;
                }
                case 38: // &
                {
                    if (!math.TryGetValueAt(i + 1, out var nextChr))
                        return Err<List<CalcElement>>("No value after &");

                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    if ((int)nextChr is not (> 96 and < 123)/*a-z*/)
                    {
                        result.Add("&");
                        break;
                    }

                    buffer.Append('&');
                    buffer.Content = BufferContentType.Arg;
                    break;
                }
                case > 47 and < 58: // 0-9
                {
                    switch (buffer.Content)
                    {
                        case BufferContentType.SpecialNumber or BufferContentType.Variable:
                        {
                            break;
                        }
                        case BufferContentType.String:
                        {
                            result1 = buffer.ParseBufferAndClear(ref opts);
                            if (result1.IsFaulted) return result1;
                            result.Add("^");
                            buffer.Content = BufferContentType.Number;
                            break;
                        }
                        case BufferContentType.Arg:
                        case BufferContentType.ArgWithValue:
                        {
                            result1 = buffer.ParseBufferAndClear(ref opts);
                            if (result1.IsFaulted) return result1;
                            buffer.Content = BufferContentType.Number;
                            break;
                        }
                        default:
                        {
                            buffer.Content = BufferContentType.Number;
                            break;
                        }
                    }

                    if (chr is '0' && buffer.Buffer.Length is 0) //Look ahead for special number
                    {
                        if (!math.TryGetValueAt(i + 1, out var nextChr))
                        {
                            buffer.Append(chr);
                            break;
                        }
                        
                        if ((int)nextChr is not (120 or 98 or 111)/*x | b | o*/)
                        {
                            buffer.Append(chr);
                            break;
                        }

                        if (!math.TryGetValueAt(i + 2, out var nextNextChr))
                        {
                            buffer.Append(chr);
                            break;
                        }

                        if ((int)nextNextChr is not (> 47 and < 58)/*0-9*/ and not (> 96 and < 103)/*a-f*/)
                        {
                            return Err<List<CalcElement>>("Invalid special number");
                        }

                        buffer.Append('0');
                        buffer.Append(nextChr);
                        buffer.Content = BufferContentType.SpecialNumber;
                        i++;
                        break;
                    }

                    buffer.Append(chr);
                    break;
                }
                case 46: // .
                {
                    if (buffer.Content is BufferContentType.Variable)
                    {
                        buffer.Append('.');
                        break;
                    }

                    if (buffer.Content is not (BufferContentType.Empty or BufferContentType.Number))
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    if (buffer.Content is BufferContentType.Empty)
                        buffer.Append('0');

                    buffer.Append('.', BufferContentType.Number);
                    break;
                }
                case > 96 and < 123: // a-z
                {
                    switch (buffer.Content)
                    {
                        case BufferContentType.Number:
                        {
                            if ((int)buffer[0] is 45)
                            {
                                if (buffer.Buffer.Length is 1)
                                {
                                    result1 = buffer.ParseBufferAndClear(ref opts);
                                    if (result1.IsFaulted) return result1;
                                    buffer.Content = BufferContentType.String;
                                    
                                    break;
                                }
                            }
                                
                            result1 = buffer.ParseBufferAndClear(ref opts);
                            if (result1.IsFaulted) return result1;
                            result.Add("*");
                            buffer.Content = BufferContentType.String;
                            break;
                        }
                        case BufferContentType.SpecialNumber:
                        case BufferContentType.String:
                        case BufferContentType.Arg:
                        case BufferContentType.ArgWithValue:
                            break;
                        case BufferContentType.Variable:
                        {
                            if ((int)buffer.Buffer[^1] is not (> 96 and < 123 or 61)/*a-z*/) //If last char is not a-z | =(a number in this case), goto default handler
                            {
                                result1 = buffer.ParseBufferAndClear(ref opts);
                                if (result1.IsFaulted) return result1;
                                buffer.Content = BufferContentType.String;
                                break;
                            }
                            
                            buffer.Content = BufferContentType.ArgWithValue;
                            break;
                        }
                        default:
                        {
                            result1 = buffer.ParseBufferAndClear(ref opts);
                            if (result1.IsFaulted) return result1;
                            buffer.Content = BufferContentType.String;
                            break;
                        }
                    }

                    buffer.Append(chr);
                    break;
                }
                case 61: // =
                {
                    if (!buffer.TryGetValueAt(0, out var firstChr))
                    {
                        if (!math.TryGetValueAt(i + 1, out var nextChr))
                            return Err<List<CalcElement>>("Invalid operator: =");

                        if ((int)nextChr is not 61/*=*/)
                            return Err<List<CalcElement>>("Invalid operator: =");

                        result.Add("==");
                        i++;
                        break;
                    }

                    if ((int)firstChr is 38/*&*/)
                    {
                        buffer.Append('=', BufferContentType.Variable);
                        break;
                    }

                    if (!math.TryGetValueAt(i + 1, out var nextChr1))
                        return Err<List<CalcElement>>("Invalid operator: =");

                    if ((int)nextChr1 is not 61)
                        return Err<List<CalcElement>>("Invalid operator: =");

                    result1 = buffer.ParseBufferAndClear(ref opts);
                    if (result1.IsFaulted) return result1;
                    result.Add("==");
                    i++;
                    break;
                }
                case 62: // >
                {
                    if (!math.TryGetValueAt(i + 1, out var nextChr))
                        return Err<List<CalcElement>>("No value after >");

                    if (i - 1 < 0)
                    {
                        var op = (int)nextChr is 61 ? ">=" : (int)nextChr is 62 ? ">>" : ">";
                        return Err<List<CalcElement>>($"No value before {op}");
                    }

                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    switch ((int)nextChr)
                    {
                        case 62:
                        {
                            result.Add(">>");
                            if (i + 2 >= math.Length)
                                return Err<List<CalcElement>>("No value after >>");
                            i++;
                            break;
                        }
                        case 61:
                        {
                            result.Add(">=");
                            if (i + 2 >= math.Length)
                                return Err<List<CalcElement>>("No value after >=");
                            i++;
                            break;
                        }
                        default:
                        {
                            result.Add(">");
                            break;
                        }
                    }

                    break;
                }
                case 60: // <
                {
                    if (!math.TryGetValueAt(i + 1, out var nextChr))
                        return Err<List<CalcElement>>("No value after <");

                    if (i - 1 < 0)
                    {
                        var op = (int)nextChr is 61 ? "<=" : (int)nextChr is 60 ? "<<" : "<";
                        return Err<List<CalcElement>>($"No value before {op}");
                    }

                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    switch ((int)nextChr)
                    {
                        case 60:
                        {
                            result.Add("<<");
                            if (i + 2 >= math.Length)
                                return Err<List<CalcElement>>("No value after <<");
                            i++;
                            break;
                        }
                        case 61:
                        {
                            result.Add("<=");
                            if (i + 2 >= math.Length)
                                return Err<List<CalcElement>>("No value after <=");
                            i++;
                            break;
                        }
                        default:
                        {
                            result.Add("<");
                            break;
                        }
                    }

                    break;
                }
                case 33: // !
                {
                    if (i - 1 < 0)
                    {
                        math.TryGetValueAt(i + 1, out var nextChr);
                        var op = (int)nextChr is 61 ? "!=" : "!";
                        return Err<List<CalcElement>>($"No value before {op}");
                    }

                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    if (math.TryGetValueAt(i + 1, out chr) && (int)chr is 61)
                    {
                        result.Add("!=");
                        i++;
                        break;
                    }

                    result.Add("!");
                    break;
                }
                case 94: // ^
                {
                    if (!math.TryGetValueAt(i + 1, out var nextChr))
                        return Err<List<CalcElement>>("No value after ^");

                    if (i - 1 < 0)
                    {
                        var op = (int)nextChr is 94 ? "^^" : "^";
                        return Err<List<CalcElement>>($"No value before {op}");
                    }

                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                    }

                    switch ((int)nextChr)
                    {
                        case 94:
                        {
                            if (i + 2 > math.Length)
                                return Err<List<CalcElement>>("No value after ^^");

                            result.Add("^^");
                            i++;
                            break;
                        }
                        default:
                        {
                            result.Add("^");
                            break;
                        }
                    }

                    break;
                }
                case 40: // (
                {
                    if (buffer.Content is BufferContentType.Number)
                    {
                        result1 = buffer.ParseBufferAndClear(ref opts);
                        if (result1.IsFaulted) return result1;
                        result.Add("*");
                    }

                    buffer.Append('(');
                    result1 = buffer.ParseBufferAndClear(ref opts);
                    if (result1.IsFaulted) return result1;
                    break;
                }
                case 91: // [
                {
                    buffer.ParseBufferAndClear(ref opts);
                    var pos = math.IndexOf(']', i + 1);
                    if (pos is -1)
                        return Err<List<CalcElement>>("No matching end square bracket");

                    var customFunction = LCalc.CustomFunction.CustomFunction.Parse(math.AsSpan()[(i + 1)..pos], functions);
                    if (customFunction.IsFaulted) return new Result<List<CalcElement>>(customFunction.Exception!);

                    functions.Add(customFunction.Value!);
                    i = pos;
                    break;
                }
                case 93: // ]
                {
                    return Err<List<CalcElement>>("No matching start square bracket");
                }
                default:
                {
                    return Err<List<CalcElement>>($"Invalid character: {chr}");
                }
            }
        }

        functions.End();

        result1 = buffer.ParseBufferAndClear(ref opts);
        if (result1.IsFaulted) return result1;

        return Ok(result);
    }

    internal static Result<double> OctalStringToDouble(string s)
    {
        var length = s.Length;
        double result = 0;
        long pow = 1;

        for (var i = 0; i < length; i++)
        {
            var chr = s[length - i - 1];
            if ((int)chr is not (> 47 and < 56))
                return Err<double>("Invalid octal number");

            result += (chr - 48) * pow;
            pow *= 8;
        }

        return Ok(result);
    }

    internal static Result<double> HexStringToDouble(string s)
    {
        var length = s.Length;
        double result = 0;
        long pow = 1;

        for (var i = 0; i < length; i++)
        {
            var chr = s[length - i - 1];

            int val;
            // ReSharper disable once RedundantCast
            switch ((int)chr)
            {
                case > 96 and < 103:
                    val = chr - 87;
                    break;
                case > 47 and < 57:
                    val = chr - 48;
                    break;
                default:
                    return Err<double>("Invalid hex number");
            }

            result += val * pow;
            pow *= 16;
        }

        return Ok(result);
    }

    internal static Result<double> BinaryStringToDouble(string s)
    {
        var length = s.Length;
        double result = 0;
        long pow = 1;

        for (var i = 0; i < length; i++)
        {
            var chr = s[length - i - 1];
            if ((int)chr is not (48 or 49))
                return Err<double>("Invalid binary number");

            result += (chr - 48) * pow;
            pow *= 2;
        }

        return Ok(result);
    }
}