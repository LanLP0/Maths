using Common.Results;
using LCalc.Extension;
using LCalc.Helpers;

namespace LCalc.CustomFunction;

internal sealed class CustomFunction
{
    private readonly List<string> _args;
    private readonly IReadOnlyList<CalcElement> _math;

    private CustomFunction(List<CalcElement> math, string name, List<string> args)
    {
        _math = math.AsReadOnly();
        Name = name;
        _args = args;
    }

    public string Name { get; }

    public static Result<CustomFunction> Parse(ReadOnlySpan<char> span, CustomFunctionCollection collection)
    {
        var pos1 = span.IndexOf('=');
        var firstHalf = span[..pos1];
        if (firstHalf[^1] is not ')')
            return Err<CustomFunction>("Invalid function signature");
        if (firstHalf.Length is 0)
            return Err<CustomFunction>("Invalid function signature");

        var pos = firstHalf.IndexOf('(');
        if (pos is -1)
            return Err<CustomFunction>("Invalid function signature");

        var name = firstHalf[..pos].ToString();
        if (name.Length is 0)
            return Err<CustomFunction>("Function name is empty");
        if (collection.ContainsName(name))
            return Err<CustomFunction>("Duplicate functions");
        var argsSpan = firstHalf[(pos + 1)..^1];

        List<string> args = new();
        if (argsSpan.Length is not 0)
        {
            Span<char> buffer = stackalloc char[argsSpan.Length];
            var count = 0;
            for (var i = 0; i < argsSpan.Length; i++)
            {
                var chr = argsSpan[i];

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
                        if (buffer[0] is not '\0')
                            args.Add(buffer.TrimEnd('\0').ToString());
                        buffer.Clear();
                        count = 0;
                        break;
                    }
                    default:
                        return Err<CustomFunction>($"Invalid character in function: '{chr}'");
                }
            }

            if (buffer[0] is not '\0')
                args.Add(buffer.TrimEnd('\0').ToString());
        }

        var secondHalf = span[(pos1 + 1)..];
        if (secondHalf.Length is 0)
            return Err<CustomFunction>("Missing function body");

        var result = SplitInputSpecial(secondHalf.ToString());
        if (result.IsFaulted)
            return new Result<CustomFunction>(result.Exception!);

        return Ok(new CustomFunction(result.Value!, name, args));
    }

    private static Result<List<CalcElement>> SplitInputSpecial(string math)
    {
        var result = new List<CalcElement>();

        var buffer = new RestrictedInputBuffer(result);

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
                        result1 = buffer.ParseBufferAndClear();
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

                    if ((int)prevChr is not (> 47 and < 58) and not (> 96 and < 123) and not (41 or 61))
                    {
                        buffer.Append('-', BufferContentType.Number);
                        break;
                    }

                    if (!math.TryGetValueAt(i + 1, out var nextChr))
                        return Err<List<CalcElement>>("No value after -");

                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear();
                        if (result1.IsFaulted) return result1;
                    }

                    if ((int)nextChr is > 47 and < 58 &&
                        (int)prevChr is not (> 47 and < 58) and not (> 96 and < 123) and not 41)
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
                        result1 = buffer.ParseBufferAndClear();
                        if (result1.IsFaulted) return result1;
                    }

                    result.Add(chr);
                    break;
                }
                case 41: // )
                {
                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear();
                        if (result1.IsFaulted) return result1;
                    }

                    result.Add(")");
                    break;
                }
                case 32: // ' '
                {
                    if (buffer.Content is not BufferContentType.Empty)
                    {
                        result1 = buffer.ParseBufferAndClear();
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
                        result1 = buffer.ParseBufferAndClear();
                        if (result1.IsFaulted) return result1;
                    }

                    if ((int)nextChr is not (> 96 and < 123)/*a-z*/)
                    {
                        result.Add("&");
                        break;
                    }
                    
                    return Err<List<CalcElement>>("Cannot have arg in function");
                }
                case > 47 and < 58: // 0-9
                {
                    switch (buffer.Content)
                    {
                        case BufferContentType.SpecialNumber:
                        {
                            break;
                        }
                        case BufferContentType.String:
                        {
                            result1 = buffer.ParseBufferAndClear();
                            if (result1.IsFaulted) return result1;
                            result.Add("^");
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
                    if (buffer.Content is not (BufferContentType.Empty or BufferContentType.Number))
                    {
                        result1 = buffer.ParseBufferAndClear();
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
                                break;
                            result1 = buffer.ParseBufferAndClear();
                            if (result1.IsFaulted) return result1;
                            result.Add("*");
                            buffer.Content = BufferContentType.String;
                            break;
                        }
                        case BufferContentType.SpecialNumber:
                        case BufferContentType.String:
                            break;
                        default:
                        {
                            result1 = buffer.ParseBufferAndClear();
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
                    if (!buffer.TryGetValueAt(0, out _))
                    {
                        if (!math.TryGetValueAt(i + 1, out var nextChr))
                            return Err<List<CalcElement>>("Invalid operator: =");

                        if ((int)nextChr is not 61)
                            return Err<List<CalcElement>>("Invalid operator: =");

                        result.Add("==");
                        i++;
                        break;
                    }

                    if (!math.TryGetValueAt(i + 1, out var nextChr1))
                        return Err<List<CalcElement>>("Invalid operator: =");

                    if ((int)nextChr1 is not 61)
                        return Err<List<CalcElement>>("Invalid operator: =");

                    result1 = buffer.ParseBufferAndClear();
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
                        result1 = buffer.ParseBufferAndClear();
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
                        result1 = buffer.ParseBufferAndClear();
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
                        result1 = buffer.ParseBufferAndClear();
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
                        result1 = buffer.ParseBufferAndClear();
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
                        result1 = buffer.ParseBufferAndClear();
                        if (result1.IsFaulted) return result1;
                        result.Add("*");
                    }

                    buffer.Append('(');
                    result1 = buffer.ParseBufferAndClear();
                    if (result1.IsFaulted) return result1;
                    break;
                }
            }
        }

        result1 = buffer.ParseBufferAndClear();
        if (result1.IsFaulted) return result1;

        return Ok(result);
    }

    public Result Run(List<CalcElement> args, CustomFunctionCollection functions)
    {
        if (_args.Count != args.Count)
            return Err("Invalid number of args");

        var math = new List<CalcElement>();
        foreach (var m in _math)
            math.Add(_args.Contains(m.StringForm) ? args[_args.IndexOf(m.StringForm)] : m.CreateCopy());

        var result = Calculator.Calculate(math, functions);
        if (result.IsFaulted)
            return result;

        if (args.Count > 1)
            args.RemoveRange(1, args.Count - 1);

        if (args.Count is not 0)
            args[0].DoubleForm = result.Value;
        else
            args.Add(result.Value);

        return Ok();
    }
}