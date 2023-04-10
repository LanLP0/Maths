using System.Text;

namespace Common.Maths.Expressions;

internal sealed class Element : IComparable<Element>
{
    public Element()
    {
        Unknowns = new Dictionary<int, int>();
    }

    public Element(double value, Dictionary<int, int> unknowns)
    {
        Value = value;
        Unknowns = unknowns;
    }

    public double Value { get; set; }

    public Dictionary<int, int> Unknowns { private set; get; }

    public int CompareTo(Element? other)
    {
        if (other is null)
            return 0;

        var max1 = 0;
        if (Unknowns.Count is not 0)
            max1 = Unknowns.Max(x => x.Value);
        var max2 = 0;
        if (other.Unknowns.Count is not 0)
            max2 = other.Unknowns.Max(x => x.Value);

        if (max1 > max2)
            return -1;

        if (max2 > max1)
            return 1;

        var sum1 = Unknowns.Select(x => x.Key * x.Value).Sum();
        var sum2 = other.Unknowns.Select(x => x.Key * x.Value).Sum();

        if (sum1 > sum2)
            return 1;

        if (sum1 < sum2)
            return -1;

        return 0;
    }

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
        var result = left.Unknowns.ToDictionary(e => e.Key, e => e.Value);

        foreach (var e in right.Unknowns)
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
            buffer.Append("[/]");

        if (Unknowns.Count is 0)
            return;

        foreach (var e in Unknowns)
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
                buffer.Append("[/]");
                continue;
            }

            buffer.Append('^');
            buffer.Append(e.Value);
            buffer.Append("[/]");
        }
    }

    public bool PowerEqual(Element val)
    {
        if (Unknowns.Count != val.Unknowns.Count)
            return false;

        foreach (var e in Unknowns)
        {
            if (!val.Unknowns.TryGetValue(e.Key, out var value))
                return false;

            if (e.Value != value)
                return false;
        }

        return true;
    }

    public Element Clone()
    {
        return new Element(Value, Unknowns.ToDictionary(e => e.Key, e => e.Value));
    }

    public void SortPower()
    {
        Unknowns = Unknowns.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
    }
}