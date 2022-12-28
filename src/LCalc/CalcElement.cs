using System.Globalization;
using Common.Results;

namespace LCalc;

internal sealed class CalcElement
{
    private double? _doubleForm;
    private string? _stringForm;

    private CalcElement(double? doubleForm, string? stringForm)
    {
        _doubleForm = doubleForm;
        _stringForm = stringForm;
    }

    public CalcElement(string text)
    {
        _stringForm = text;
    }

    public CalcElement(double num)
    {
        _doubleForm = num;
    }

    public bool IsString => !_doubleForm.HasValue;

    public bool IsNumber
    {
        get
        {
            _doubleForm ??= double.TryParse(_stringForm, out var num)
                ? num
                : null;
            return _doubleForm.HasValue;
        }
    }

    public bool IsInt
    {
        get
        {
            _doubleForm ??= double.TryParse(_stringForm, out var num)
                ? num
                : null;
            return _doubleForm.HasValue && Math.Abs(_doubleForm.Value % 1) < double.Epsilon * 100;
        }
    }

    public double DoubleForm
    {
        private get
        {
            _doubleForm ??= double.Parse(_stringForm!);
            return _doubleForm.Value;
        }
        set
        {
            _doubleForm = value;
            _stringForm = null;
        }
    }

    public string StringForm
    {
        get
        {
            _stringForm ??= _doubleForm!.Value.ToString(CultureInfo.InvariantCulture);
            return _stringForm!;
        }
        set
        {
            _stringForm = value;
            _doubleForm = null;
        }
    }

    public bool StringEq(string other)
    {
        if (!IsString)
            return false;

        return _stringForm! == other;
    }

    public int Length => StringForm.Length;

    public static implicit operator string(CalcElement e)
    {
        return e.StringForm;
    }

    public static implicit operator CalcElement(string text)
    {
        return new CalcElement(text);
    }

    public static implicit operator CalcElement(char chr)
    {
        return chr.ToString();
    }

    public static implicit operator CalcElement(double num)
    {
        return new CalcElement(num);
    }

    public double RiskyGetValue()
    {
        return DoubleForm;
    }

    public Result<double> GetValue()
    {
        if (!IsNumber)
            return Err<double>($"{StringForm} is not a number");

        return Ok(DoubleForm);
    }

    public Result<long> AsInt64()
    {
        if (!IsInt)
            return Err<long>($"{StringForm} is not an integer");
        if (DoubleForm is > long.MaxValue or < long.MinValue)
            return Err<long>(new OverflowException());

        return Ok((long)DoubleForm);
    }

    public Result<int> AsInt()
    {
        if (!IsInt)
            return Err<int>($"{StringForm} is not an integer");
        if (DoubleForm is > int.MaxValue or < int.MinValue)
            return Err<int>(new OverflowException());

        return Ok((int)DoubleForm);
    }

    public override string ToString()
    {
        return StringForm;
    }

    public bool StartsWith(char value)
    {
        if (!IsString)
            return false;

        return StringForm.StartsWith(value);
    }

    public bool Contains(char value)
    {
        return StringForm.Contains(value);
    }

    public string Substring(int startIndex, int length)
    {
        return StringForm.Substring(startIndex, length);
    }

    public int IndexOf(char value)
    {
        return StringForm.IndexOf(value);
    }

    public bool EndsWith(char value)
    {
        if (_doubleForm.HasValue)
            return false;

        return StringForm.EndsWith(value);
    }

    public CalcElement CreateCopy()
    {
        return new CalcElement(_doubleForm, _stringForm);
    }
}