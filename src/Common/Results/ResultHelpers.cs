namespace Common.Results;

internal static class ResultHelpers
{
    public static Result Ok()
    {
        return new Result();
    }

    public static Result<T> Err<T>(string errMsg)
    {
        return Err<T>(new Exception(errMsg));
    }

    public static Result<T> Err<T>(Exception err)
    {
        return new Result<T>(err);
    }

    public static Result Err()
    {
        return new Result(new Exception());
    }

    public static Result Err(string errMsg)
    {
        return new Result(new Exception(errMsg));
    }
}