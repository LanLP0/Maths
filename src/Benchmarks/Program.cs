using System.Text;
using BenchmarkDotNet.Running;
using Benchmarks;
using LCalc;
using LCalc.MathTree;

// var l = new LCalcBenchmarks();
//
// l.Math = "1233211 + 227633 * 6555 + 999 / (99+1)";
// l.Setup();

// Console.WriteLine(l.LCalc());
// Console.WriteLine(l.LCalcRaw());

// BenchmarkRunner.Run<Benchmarks.Benchmarks>();

BenchmarkRunner.Run<MiscBenchmarks>();

// unsafe
// {
//     Console.WriteLine(sizeof(Result));
//     Console.WriteLine(sizeof(ResultDouble));
//     Console.WriteLine(sizeof(Result<double>));
// }