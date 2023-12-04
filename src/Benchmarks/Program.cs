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

// for (;;)
// {
//     Console.Write("Input: ");
//     var input = Console.ReadLine()!;
//
//     var parser = new ExpParser();
//     var result = parser.Parse(input);
//     if (result.Faulted)
//     {
//         Console.WriteLine(result.Exception);
//         continue;
//     }
//     
//     var buffer = new StringBuilder();
//     result = parser.GetTopNode().RenderStep(buffer, -1, parser.Scope, 0);
//     if (result.Faulted)
//     {
//         Console.WriteLine(result.Exception);
//         continue;
//     }
//
//     Console.Write("ExpParser:  ");
//     Console.WriteLine(buffer.ToString());
//     var result1 = parser.GetTopNode().Calc(parser.Scope);
//     Console.WriteLine(result1.Value);
//     
//     var oldParserResult = Calculator.CalcRaw(input + " &render");
//     Console.Write("Old Parser:  ");
//     Console.WriteLine(oldParserResult.Steps);
//     Console.WriteLine(oldParserResult.RenderValue());
// }