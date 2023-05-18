using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Common.Results;
using static Common.Results.ResultHelpers;

namespace Benchmarks;

// [MemoryDiagnoser]
[MaxIterationCount(50)]
public class MiscBenchmarks
{
    // [Params(1.123, 1230.1, 1020304050.321)]
    // public double Num1 { get; set; }
    //
    // [Params(1.123, 1230.1, 1020304050.321)]
    // public double Num2 { get; set; }
    //
    // [Benchmark(Baseline = true)]
    // public double Add()
    // {
    //     return Num1 + Num2;
    // }
    //
    // [Benchmark]
    // public double Subtract()
    // {
    //     return Num1 - Num2;
    // }
    //
    // [Benchmark]
    // public double Multiply()
    // {
    //     return Num1 * Num2;
    // }
    //
    // [Benchmark]
    // public double Divide()
    // {
    //     return Num1 / Num2;
    // }
    //
    // [Benchmark]
    // public double Modulo()
    // {
    //     return Num1 % Num2;
    // }

    [Benchmark]
    public Result<double> ResultTCreate()
    {
        return 1.1;
    }

    [Benchmark]
    public ResultDouble ResultDoubleCreate()
    {
        return new ResultDouble(1.1);
    }

    [Benchmark]
    public Result2<double> Result2TCreate()
    {
        return new Result2<double>(1.1);
    }

    [Benchmark(Baseline = true)]
    public Result ResultCreate()
    {
        return Ok();
    }
}

public readonly ref struct ResultDouble
{
    public readonly double? Value;
    public readonly Exception? Exception;
    public bool Success => !Faulted;
    public bool Faulted => Exception is null;

    public ResultDouble(double value)
    {
        Value = value;
        Exception = null;
    }

    public ResultDouble(Exception exception)
    {
        Value = default;
        Exception = exception;
    }

    public static implicit operator ResultDouble(Exception exception)
    {
        return new ResultDouble(exception);
    }
}

public readonly ref struct Result2<T>
{
    public readonly T? Value;
    public readonly Exception? Exception;
    public bool Success => !Faulted;
    public bool Faulted => Exception is not null;

    public Result2(Exception exception)
    {
        Exception = exception;
        Value = default;
    }

    public Result2(T value)
    {
        Exception = null;
        Value = value;
    }

    public T UnwrapOr(T defaultValue)
    {
        return Value ?? defaultValue;
    }

    public static implicit operator Result2<T>(T value)
    {
        return new Result2<T>(value);
    }

    public static implicit operator Result2<T>(Exception exception)
    {
        return new Result2<T>(exception);
    }

    public override string ToString()
    {
        return Success
            ? Value?.ToString() ?? "(null)"
            : Exception?.ToString() ?? "(error)";
    }
}