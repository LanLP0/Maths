using System.Text;

namespace Common.Maths.Expressions;

public class Element : IComparable<Element>
{
    public Element()
    {
        Powers = new Dictionary<int, int>();
    }

    public Element(double value, Dictionary<int, int> powers)
    {
        Value = value;
        Powers = powers;
    }

    public double Value { get; set; }

    public Dictionary<int, int> Powers { private set; get; }

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

    public void RenderToBufferWithColor(StringBuilder buffer, bool isSelected)
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
            if (!val.Powers.TryGetValue(e.Key, out var value))
                return false;

            if (e.Value != value)
                return false;
        }

        return true;
    }

    public Element Clone()
    {
        return new Element(Value, Powers.ToDictionary(e => e.Key, e => e.Value));
    }

    public void SortPower()
    {
        Powers = Powers.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
    }

    public int CompareTo(Element? other)
    {
        if (other is null)
            return 0;

        var max1 = 0;
        if (Powers.Count is not 0)
            max1 = Powers.Max(x => x.Value);
        var max2 = 0;
        if (other.Powers.Count is not 0)
            max2 = other.Powers.Max(x => x.Value);

        if (max1 > max2)
            return -1;

        if (max2 > max1)
            return 1;

        var sum1 = Powers.Select(x => x.Key * x.Value).Sum();
        var sum2 = other.Powers.Select(x => x.Key * x.Value).Sum();

        if (sum1 > sum2)
            return 1;

        if (sum1 < sum2)
            return -1;
        
        return 0;
    }
}