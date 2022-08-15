using System.Text;
using Common.Results;
using LCalc.Extension;
using LCalc.Helpers;

namespace LCalc.CustomFunction;

internal ref struct RestrictedInputBuffer
{
    public RestrictedInputBuffer(List<CalcElement> list)
    {
        Buffer = new StringBuilder();
        List = list;
    }

    public BufferContentType Content { get; set; } = BufferContentType.Empty;

    public StringBuilder Buffer { get; init; }
    public List<CalcElement> List { get; init; }

    public Result ParseBufferAndClear()
    {
        if (Buffer.Length is 0)
            return new Result();

        switch (Content)
        {
            case BufferContentType.Number:
            {
                var num = Buffer.ToString();

                if (num is "-")
                {
                    if (List[^1].StringEq("+"))
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

                if (num.Length is 2)
                    return Err("Empty number");

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
        return Ok();
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

    public char this[int index] => Buffer[index];
}