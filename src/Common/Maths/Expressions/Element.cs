using System.Text;

namespace Common.Maths.Expressions;

internal class Element
{
    public double Value { get; set; }

    public List<int> Powers { get; private set; } = new();

    public static Element operator *(Element left, Element right)
    {
        return new Element
        {
            Value = left.Value * right.Value,
            Powers = MulPowerFromElements(left, right)
        };
    }

    private static List<int> MulPowerFromElements(Element left, Element right)
    {
        var lenght = Math.Max(left.Powers.Count, right.Powers.Count);
        var result = new List<int>();

        for (var i = 0; i < lenght; i++)
        {
            var p = 0;
            if (i < left.Powers.Count)
                p = left.Powers[i];
            
            var p1 = 0;
            if (i < right.Powers.Count)
                p1 = right.Powers[i];
            
            result.Add(p + p1);
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

        for (var i = 0; i < Powers.Count; i++)
        {
            var power = Powers[i];

            if (power is 0)
                continue;
            
            buffer.Append("[Cyan]");

            buffer.Append((char)(i % 26 + 97)); // Turn i into character from a-z

            // Loop over variable name
            var mod = i / 26;
            if (mod >= 2)
                buffer.Append(mod);

            if (power is 1)
            {
                buffer.Append("[/Cyan]");
                continue;
            }

            buffer.Append('^');
            buffer.Append(power);
            buffer.Append("[/Cyan]");
        }
    }
}