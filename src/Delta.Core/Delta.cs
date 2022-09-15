namespace Delta.Core;

internal sealed class Delta
{
    private readonly List<SimpleVariable> _l0 = new();
    private readonly List<SimpleVariable> _l1 = new();
    private readonly List<SimpleVariable> _l2 = new();
    public (SimpleVariable APart, SimpleVariable NumPart) V0 { get; init; }
    public (SimpleVariable APart, SimpleVariable NumPart) V1 { get; init; }
    public (SimpleVariable APart, SimpleVariable NumPart) V2 { get; init; }

    public FinalDelta Calc()
    {
        HandlePower2(V1.APart, V1.NumPart);

        _l2.Add(new SimpleVariable
        {
            NumberPart = V0.APart.NumberPart * V2.APart.NumberPart * -4
        });
        _l1.Add(new SimpleVariable
        {
            NumberPart = V0.APart.NumberPart * V2.NumPart.NumberPart * -4
        });
        _l1.Add(new SimpleVariable
        {
            NumberPart = V0.NumPart.NumberPart * V2.APart.NumberPart * -4
        });
        _l0.Add(new SimpleVariable
        {
            NumberPart = V0.NumPart.NumberPart * V2.NumPart.NumberPart * -4
        });

        return new FinalDelta
        {
            T0 = new SimpleVariable { NumberPart = _l2.Sum(a => a.NumberPart) },
            T1 = new SimpleVariable { NumberPart = _l1.Sum(a => a.NumberPart) },
            T2 = new SimpleVariable { NumberPart = _l0.Sum(a => a.NumberPart) }
        };
    }

    private void HandlePower2(SimpleVariable aPart, SimpleVariable numPart)
    {
        var middle = new SimpleVariable { NumberPart = aPart.NumberPart * numPart.NumberPart * 2 };

        numPart.NumberPart = (int)Math.Pow(numPart.NumberPart, 2);
        aPart.NumberPart = (int)Math.Pow(aPart.NumberPart, 2);

        _l2.Add(aPart);
        _l0.Add(numPart);
        _l1.Add(middle);
    }
}