using System.Text;

namespace Common.Cli.Maths.Expressions;

internal class Element
{
    public Element()
    {
        Powers = new();
    }

    public Element(double value, Dictionary<int, int> powers)
    {
        Value = value;
        Powers = powers;
    }

    public double Value { get; set; }

    public Dictionary<int, int> Powers { get; private set; }

    public static Element operator *(Element left, Element right)
    {
        return new Element
        (
            left.Value * right.Value,
            MulPowerFromElements(left, right)
        );
    }

    private static Dictionary<int, int> MulPowerFromElements(Element left, Element right)
    {
        var result = left.Powers.ToDictionary(e => e.Key, e => e.Value);
        
        foreach (var e in right.Powers)
        {
            if (result.ContainsKey(e.Key))
            {
                result[e.Key] += e.Value;
                continue;
            }

            result.Add(e.Key, e.Value);
        }

        return result;
    }

    public void RenderBufferWithColor(StringBuilder buffer, bool isSelected)
    {
        if (isSelected)
            buffer.Append("[Green]");

        buffer.Append(Value);

        if (isSelected)
            buffer.Append("[/Green]");

        if (Powers.Count is 0)
            return;

        foreach (var e in Powers)
        {
            if (e.Value is 0)
                continue;
            
            buffer.Append("[Cyan]");

            buffer.Append((char)(e.Key % 26 + 97)); // Turn i into character from a-z

            // Loop over variable name
            var mod = e.Key / 26;
            if (mod >= 2)
                buffer.Append(mod);

            if (e.Value is 1)
            {
                buffer.Append("[/Cyan]");
                continue;
            }

            buffer.Append('^');
            buffer.Append(e.Value);
            buffer.Append("[/Cyan]");
        }
    }

    public bool PowerEqual(Element val)
    {
        if (Powers.Count != val.Powers.Count)
            return false;

        foreach (var e in Powers)
        {
            if (!val.Powers.ContainsKey(e.Key))
                return false;

            if (e.Value != val.Powers[e.Key])
                return false;
        }

        return true;
    }

    public Element Clone()
    {
        return new(Value, Powers.ToDictionary(e => e.Key, e => e.Value));
    }
}