using System.Text;
using LCalc.Helpers;

namespace LCalc.Helpers;

internal ref struct InputBuffer
{
    public InputBuffer(List<CalcElement> list, Dictionary<string, CalcElement> args)
    {
        Buffer = new StringBuilder();
        this.List = list;
        Args = args;
    }

    public BufferContentType Content { get; set; } = BufferContentType.Empty;

    public StringBuilder Buffer { get; init; }
    public List<CalcElement> List { get; init; }
    public Dictionary<string, CalcElement> Args { get; init; }
    
    public Result ParseBufferAndClear(ref CalcOptions opts)
    {
        if (Buffer.Length is 0)
            return new();
        
        switch (Content)
        {
            case BufferContentType.Arg:
            {
                var arg = Buffer.ToString();
                Buffer.Clear();

                if (!arg.Contains('='))
                {
                    switch (arg.Substring(1))
                    {
                        case "step":
                            opts.StepByStep = true;
                            break;
                        case "raw":
                            opts.Raw = true;
                            break;
                    }
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

                if (!Args.TryAdd(tmp[0], tmp[1]))
                {
                    return Err("Variable has already been set");
                }

                break;
            }
            case BufferContentType.SpecialNumber:
            {
                var num = Buffer.ToString();
                Buffer.Clear();

                switch ((int) num[1])
                {
                    case 104: // h
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
        return new();
    }

    public void Append(char c) => Buffer.Append(c);

    public void Append(char c, BufferContentType contentType)
    {
        Buffer.Append(c);
        Content = contentType;
    }

    public bool TryGetValueAt(int index, out char value) => Buffer.TryGetValueAt(index, out value);
#if DEBUG
    public override string ToString() => Buffer.ToString();
#endif

    public char this[int index] => Buffer[index];
}