namespace Common.Results;

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

    public static implicit operator T?(Result<T> value)
    {
        return value.Value;
    }

    public static implicit operator Result<T>(T value)
    {
        return new Result<T>(value);
    }

    public static implicit operator Result<T>(Result value)
    {
        return new Result<T>(value);
    }

    public static implicit operator Result<T>(Exception exception)
    {
        return new Result<T>(exception);
    }

    public static implicit operator Result(Result<T> value)
    {
        return value._innerResult;
    }

    public override string ToString()
    {
        return Success
            ? Value?.ToString() ?? "(null)"
            : Exception?.ToString() ?? "(error)";
    }
}

public readonly ref struct Result
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

    public static implicit operator Result(Exception exception)
    {
        return new Result(exception);
    }
}