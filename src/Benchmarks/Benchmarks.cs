using BenchmarkDotNet.Attributes;

namespace Benchmarks;

[MemoryDiagnoser]
public class Benchmarks
{
    // [Benchmark]
    // public void GetPrime1000()
    // {
    //     var curr = 0;
    //     for (var i = 0; i < 1000; i++)
    //     {
    //         curr = Maths.Maths.GetNextPrime(curr);
    //     }
    // }
    // [Benchmark]
    // public void GetPrime100()
    // {
    //     var curr = 0;
    //     for (var i = 0; i < 100; i++)
    //     {
    //         curr = Maths.Maths.GetNextPrime(curr);
    //     }
    // }
    // [Benchmark]
    // public void GetPrime10()
    // {
    //     var curr = 0;
    //     for (var i = 0; i < 10; i++)
    //     {
    //         curr = Maths.Maths.GetNextPrime(curr);
    //     }
    // }

    // [Benchmark(Baseline = true)]
    // public void IsPrimeInternal()
    // {
    //     Maths.Maths.IsPrimeInternal(997);
    // }

    // [Benchmark]
    // public void IsPrime()
    // {
    //     Maths.Maths.IsPrime(997);
    // }
}