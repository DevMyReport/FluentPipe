using BenchmarkDotNet.Running;
using FluentPipe.Benchmarks.Runners;

//BenchmarkRunner.Run<FluentPipeInlineBenchmark>();
BenchmarkRunner.Run<FluentPipeParallelBenchmark>();
