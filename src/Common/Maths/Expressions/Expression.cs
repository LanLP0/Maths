using System.Text;

namespace Common.Maths.Expressions;

internal class Expression
{
    public List<Element> Elements { get; private set; } = new();

    public static Expression operator *(Expression left, Expression right)
    {
        var ex = new Expression();
        
        foreach (var element in left.Elements)
        {
            foreach (var element1 in right.Elements)
            {
                ex.Elements.Add(element * element1);
            }
        }

        return ex.Collapse();
    }

    public Expression Collapse()
    {
        if (Elements.Count <= 1)
            return this;

        for (var i = 0; i < Elements.Count;)
        {
            var e = Elements[i];

            for (var j = ++i; j < Elements.Count; j++)
            {
                var e1 = Elements[j];
                if (!e.Powers.SequenceEqual(e1.Powers))
                    continue;

                e.Value += e1.Value;
                Elements.RemoveAt(j);
            }
        }

        return this;
    }

    public string ToStringWithColor(int selectedPos = -1)
    {
        var buffer = new StringBuilder();

        for (var i = 0; i < Elements.Count;)
        {
            Elements[i].RenderBufferWithColor(buffer, selectedPos == i);

            if (++i < Elements.Count)
            {
                buffer.Append(" + ");
            }
        }

        return buffer.ToString();
    }
}