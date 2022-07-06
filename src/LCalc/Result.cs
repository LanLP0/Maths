// ReSharper disable MemberCanBePrivate.Global
namespace LCalc;

internal readonly ref struct Result<T>
{
    private readonly Result _innerResult;
    public readonly T? Value;
    public Exception? Exception => _innerResult.Exception;
    public bool Success => !IsFaulted;
    public bool IsFaulted => _innerResult.IsFaulted;

    private Result(Result innerResult)
    {
        _innerResult = innerResult;
        Value = default;
    }

    public Result(Exception exception)
    {
        _innerResult = new Result(exception);
        Value = default;
    }

    public Result(T value)
    {
        _innerResult = new Result();
        Value = value;
    }

    public static implicit operator T?(Result<T> value) => value.Value;
    
    public static implicit operator Result<T>(T value) => new(value);
    
    public static implicit operator Result<T>(Result value) => new(value);
    
    public static implicit operator Result<T>(Exception exception) => new(exception);
    
    public static implicit operator Result(Result<T> value) => value._innerResult;

    public override string ToString() => Success
        ? Value?.ToString() ?? "(null)"
        : Exception?.ToString() ?? "(error)";
}

internal readonly ref struct Result
{
    public readonly Exception? Exception;
    public bool Success => !IsFaulted;
    public readonly bool IsFaulted;

    public Result()
    {
        Exception = null;
        IsFaulted = false;
    }

    public Result(Exception exception)
    {
        Exception = exception;
        IsFaulted = true;
    }
    
    public static implicit operator Result(Exception exception) => new(exception);
}