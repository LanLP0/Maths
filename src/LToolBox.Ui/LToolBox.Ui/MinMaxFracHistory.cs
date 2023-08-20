using System.Text;

namespace LToolBox.Ui;

public class MinMaxFracHistory
{
    public double T0 { get; }
    public double T1 { get; }
    public double T2 { get; }

    public double B0 { get; }
    public double B1 { get; }
    public double B2 { get; }
    
    public string FracTopText { get; }
    public string FracBottomText { get; }
    public string PolynomialText { get; }
    public string ResultText { get; }

    public MinMaxFracHistory(double t0, double t1, double t2, double b0, double b1, double b2, double v0, double v1,
        double v2, string resultText)
    {
        T0 = t0;
        T1 = t1;
        T2 = t2;
        B0 = b0;
        B1 = b1;
        B2 = b2;

        var buffer = new StringBuilder();
        
        if (t0 is not 0)
        {
            buffer.Append(t0);
            buffer.Append("x^2");
            
            if (t1 is not 0 || t2 is not 0)
                buffer.Append(" + ");
        }

        if (t1 is not 0)
        {
            buffer.Append(t1);
            buffer.Append('x');

            if (t2 is not 0)
                buffer.Append(" + ");
        }

        if (t2 is not 0)
            buffer.Append(t2);

        FracTopText = buffer.ToString();
        buffer.Clear();
            
        if (b0 is not 0)
        {
            buffer.Append(b0);
            buffer.Append("x^2");
            
            if (b1 is not 0 || b2 is not 0)
                buffer.Append(" + ");
        }

        if (b1 is not 0)
        {
            buffer.Append(b1);
            buffer.Append('x');

            if (b2 is not 0)
                buffer.Append(" + ");
        }

        if (b2 is not 0)
            buffer.Append(b2);

        FracBottomText = buffer.ToString();
        buffer.Clear();
        
        if (v0 is not 0)
        {
            buffer.Append(v0);
            buffer.Append("A^2");
            
            if (v1 is not 0 || v2 is not 0)
                buffer.Append(" + ");
        }

        if (v1 is not 0)
        {
            buffer.Append(v1);
            buffer.Append('A');

            if (v2 is not 0)
                buffer.Append(" + ");
        }

        if (v2 is not 0)
            buffer.Append(v2);
        
        PolynomialText = $"-71A^2 + 52A + -8 >= 0";
        
        ResultText = resultText;
    }
}