namespace LToolBox.Ui;

public sealed class MathHistory
{
    public MathHistory(string math, string result)
    {
        Math = math;
        Result = result;
    }

    public string Math { get; set; }
    public string Result { get; set; }
}