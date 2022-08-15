using System.Text;
using Common.Results;
using LCalc.Extension;
using LCalc.Helpers;

namespace LCalc;

internal ref struct InputBuffer
{
    public InputBuffer(List<CalcElement> list, Dictionary<string, CalcElement> args)
    {
        Buffer = new StringBuilder();
        List = list;
        Args = args;
    }

    public BufferContentType Content { get; set; } = BufferContentType.Empty;

    public StringBuilder Buffer { get; init; }
    public List<CalcElement> List { get; init; }
    public Dictionary<string, CalcElement> Args { get; init; }

    public Result ParseBufferAndClear(ref CalcOptions opts)
    {
        if (Buffer.Length is 0)
            return new Result();

        switch (Content)
        {
            case BufferContentType.Arg:
            {
                var arg = Buffer.ToString();
                Buffer.Clear();

                if (!arg.Contains('='))
                    switch (arg.Substring(1))
                    {
                        case "step":
                            opts.StepByStep = true;
                            break;
                        case "raw":
                            opts.Raw = true;
                            break;
                        default:
                            return Err("Not an valid arg");
                    }

                break;
            }
            case BufferContentType.Variable:
            {
                var arg = Buffer.ToString();
                Buffer.Clear();
                var tmp = arg.Substring(1).Split('=');

                if (string.IsNullOrEmpty(tmp[1]))
                    return Err("Missing variable value");

                if (!double.TryParse(tmp[1], out var val))
                    return Err("Variable's value is not a valid number");
                    
                if (!Args.TryAdd(tmp[0], val)) return Err("Variable has already been set");

                break;
            }
            case BufferContentType.ArgWithValue:
            {
                var arg = Buffer.ToString();
                Buffer.Clear();
                var tmp = arg.Substring(1).Split('=');

                if (string.IsNullOrEmpty(tmp[1]))
                    return Err("Missing arg value");

                switch (tmp[0])
                {
                    // case "solve":
                    // {
                    //     opts.SolveFor.Add(tmp[1]);
                    //     break;
                    // }
                    default:
                        return Err("Not an valid arg");
                }

                break;
            }
            case BufferContentType.Number:
            {
                var num = Buffer.ToString();

                if (num is "-")
                {
                    if (List.Count > 0 && List[^1].StringEq("+"))
                        List.Remove(List.Count - 1);
                    
                    List.Add("-");

                    Buffer.Clear();
                    break;
                }

                if (!double.TryParse(num, out var val))
                    return Err("Invalid number");
                
                List.Add(val);

                Buffer.Clear();
                break;
            }
            case BufferContentType.SpecialNumber:
            {
                var num = Buffer.ToString();
                Buffer.Clear();

                switch ((int)num[1])
                {
                    case 120: // x
                    {
                        var result = CalculatorHelpers.HexStringToDouble(num.Substring(2));
                        if (result.IsFaulted)
                            return result;

                        List.Add(result.Value);
                        break;
                    }
                    case 98: // b
                    {
                        var result = CalculatorHelpers.BinaryStringToDouble(num.Substring(2));
                        if (result.IsFaulted)
                            return result;

                        List.Add(result.Value);
                        break;
                    }
                    case 111: // o
                    {
                        var result = CalculatorHelpers.OctalStringToDouble(num.Substring(2));
                        if (result.IsFaulted)
                            return result;

                        List.Add(result.Value);
                        break;
                    }
                }

                break;
            }
            default:
            {
                List.Add(Buffer.ToString());
                Buffer.Clear();
                break;
            }
        }

        Content = BufferContentType.Empty;
        return new Result();
    }

    public void Append(char c)
    {
        Buffer.Append(c);
    }

    public void Append(char c, BufferContentType contentType)
    {
        Buffer.Append(c);
        Content = contentType;
    }

    public bool TryGetValueAt(int index, out char value)
    {
        return Buffer.TryGetValueAt(index, out value);
    }
#if DEBUG
    public override string ToString()
    {
        return Buffer.ToString();
    }
#endif

    public char this[int index] => Buffer[index];
}