namespace Common.Results;

public readonly ref struct Result<T>
{
    private readonly Result _innerResult;

    public T? Value { get; }
    public Exception? Exception => _innerResult.Exception;
    public bool Success => !Faulted;
    public bool Faulted => _innerResult.Faulted;

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

    public T UnwrapOr(T defaultValue)
    {
        return Value ?? defaultValue;
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
    public bool Success => !Faulted;
    public readonly bool Faulted;

    public Result()
    {
        Exception = null;
        Faulted = false;
    }

    public Result(Exception exception)
    {
        Exception = exception;
        Faulted = true;
    }

    public static implicit operator Result(Exception exception)
    {
        return new Result(exception);
    }
}