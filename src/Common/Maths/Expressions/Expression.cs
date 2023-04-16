using System.Text;

namespace Common.Maths.Expressions;

internal sealed class Expression
{
    public List<Element> Values { get; private set; } = new();

    public static Expression operator *(Expression left, Expression right)
    {
        var ex = new Expression();

        foreach (var element in left.Values)
        foreach (var element1 in right.Values)
            ex.Values.Add(element * element1);

        return ex.Condense();
    }

    public static Expression operator -(Expression left, Expression right)
    {
        var ex = new Expression();

        ex.Values = left.Clone().Values;

        var rightCloned = right.Clone();
        foreach (var e in rightCloned.Values)
        {
            e.Value = -e.Value;
            ex.Values.Add(e);
        }

        return ex.Condense();
    }

    public static Expression operator +(Expression left, Expression right)
    {
        var ex = new Expression();

        ex.Values = left.Clone().Values;

        ex.Values.AddRange(right.Clone().Values);

        return ex.Condense();
    }

    /// <summary>
    ///     Validate every value(s) is not 0
    /// </summary>
    /// <returns>true if every value(s) is not 0; false otherwise</returns>
    public bool Validate()
    {
        foreach (var value in Values)
            if (Math.Abs(value.Value) < double.Epsilon)
                return false;

        return true;
    }

    public static Expression Pow(Expression left, int right)
    {
        var ex = left.Clone();

        for (var i = 1; i < right; i++) ex *= left;

        return ex.Condense();
    }

    public Expression Condense()
    {
        if (Values.Count <= 1)
            return this;

        for (var i = 0; i < Values.Count;)
        {
            var e = Values[i];

            for (var j = ++i; j < Values.Count; j++)
            {
                var e1 = Values[j];
                if (!e.PowerEqual(e1))
                    continue;

                e.Value += e1.Value;
                Values.RemoveAt(j);
            }

            if (e.Value is 0)
                Values.RemoveAt(--i);
        }

        return this;
    }

    public string ToMarkupColorString(int selectedPos = -1)
    {
        var buffer = new StringBuilder();

        for (var i = 0; i < Values.Count;)
        {
            Values[i].RenderToBufferWithColor(buffer, selectedPos == i);

            if (++i < Values.Count) buffer.Append(" + ");
        }

        return buffer.ToString();
    }

    private Expression Clone()
    {
        var ex = new Expression();
        foreach (var e in Values) ex.Values.Add(e.Clone());

        return ex;
    }

    public void Sort()
    {
        Values.Sort();

        foreach (var e in Values) e.SortPower();
    }
}