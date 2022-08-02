using System.Text;

namespace Common.Maths.Expressions;

internal class Expression
{
    public List<Element> Elements { get; private set; } = new();

    public static Expression operator *(Expression left, Expression right)
    {
        var ex = new Expression();

        foreach (var element in left.Elements)
        foreach (var element1 in right.Elements)
            ex.Elements.Add(element * element1);

        return ex.Collapse();
    }

    public static Expression operator -(Expression left, Expression right)
    {
        var ex = new Expression();

        ex.Elements = left.Clone().Elements;

        var rightCloned = right.Clone();
        foreach (var e in rightCloned.Elements)
        {
            e.Value = -e.Value;
            ex.Elements.Add(e);
        }

        return ex.Collapse();
    }

    public static Expression operator +(Expression left, Expression right)
    {
        var ex = new Expression();

        ex.Elements = left.Clone().Elements;

        ex.Elements.AddRange(right.Clone().Elements);

        return ex.Collapse();
    }

    public static Expression Pow(Expression left, int right)
    {
        var ex = left.Clone();

        for (var i = 1; i < right; i++) ex *= left;

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
                if (!e.PowerEqual(e1))
                    continue;

                e.Value += e1.Value;
                Elements.RemoveAt(j);
            }

            if (e.Value is 0)
                Elements.RemoveAt(--i);
        }

        return this;
    }

    public string ToStringWithColor(int selectedPos = -1)
    {
        var buffer = new StringBuilder();

        for (var i = 0; i < Elements.Count;)
        {
            Elements[i].RenderToBufferWithColor(buffer, selectedPos == i);

            if (++i < Elements.Count) buffer.Append(" + ");
        }

        return buffer.ToString();
    }

    private Expression Clone()
    {
        var ex = new Expression();
        foreach (var e in Elements) ex.Elements.Add(e.Clone());

        return ex;
    }

    public void Sort()
    {
        Elements.Sort();

        foreach (var e in Elements)
        {
            e.SortPower();
        }
    }
}