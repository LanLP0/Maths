namespace Common.Results;

public readonly ref struct Result<T>
{
    public T? Value { get; }
    public Exception? Exception { get; }
    public bool Success => Exception is null;
    public bool Faulted => !Success;

    private Result(Result result)
    {
        Exception = result.Exception;
        Value = default;
    }

    public Result(Exception exception)
    {
        Exception = exception;
        Value = default;
    }

    public Result(T value)
    {
        Exception = null;
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
        if (value.Success)
            return new Result();

        return new Result(value.Exception!);
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