namespace LCalc.Helpers;

internal static class ResultHelpers
{
    public static Result<T> Ok<T>(T value)
    {
        return new Result<T>(value);
    }

    public static Result<T> Err<T>(string errMsg) =>
        Err<T>(new Exception(errMsg));
    
    public static Result<T> Err<T>(Exception err)
    {
        return new Result<T>(err);
    }
    
    public static Result Ok()
    {
        return new Result();
    }

    public static Result Err(string errMsg) =>
        Err(new Exception(errMsg));
    
    public static Result Err(Exception err)
    {
        return new Result(err);
    }
}