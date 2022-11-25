using BenchmarkDotNet.Running;
using Benchmarks;

// var l = new LCalcBenchmarks();
//
// l.Math = "1233211 + 227633 * 6555 + 999 / (99+1)";
// l.Setup();

// Console.WriteLine(l.LCalc());
// Console.WriteLine(l.LCalcRaw());

BenchmarkRunner.Run<LCalcBenchmarks>();

// BenchmarkRunner.Run<Benchmarks.Benchmarks>();
