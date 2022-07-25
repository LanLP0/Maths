using System.Text;
using Common.Results;

namespace LCalc.Helpers.CustomFunction;

internal ref struct SpecialInputBuffer
{
    public SpecialInputBuffer(List<CalcElement> list)
    {
        Buffer = new StringBuilder();
        this.list = list;
    }

    public BufferContentType Content { get; set; } = BufferContentType.Empty;

    public StringBuilder Buffer { get; init; }
    public List<CalcElement> list { get; init; }

    public Result ParseBufferAndClear()
    {
        if (Buffer.Length is 0)
            return new Result();

        switch (Content)
        {
            case BufferContentType.SpecialNumber:
            {
                var num = Buffer.ToString();
                Buffer.Clear();

                if (num.Length is 2)
                    return Err("Empty number");

                switch ((int)num[1])
                {
                    case 104: // h
                    {
                        var result = CalculatorHelpers.HexStringToDouble(num.Substring(2));
                        if (result.IsFaulted)
                            return result;

                        list.Add(result.Value);
                        break;
                    }
                    case 98: // b
                    {
                        var result = CalculatorHelpers.BinaryStringToDouble(num.Substring(2));
                        if (result.IsFaulted)
                            return result;

                        list.Add(result.Value);
                        break;
                    }
                    case 111: // o
                    {
                        var result = CalculatorHelpers.OctalStringToDouble(num.Substring(2));
                        if (result.IsFaulted)
                            return result;

                        list.Add(result.Value);
                        break;
                    }
                }

                break;
            }
            default:
            {
                list.Add(Buffer.ToString());
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