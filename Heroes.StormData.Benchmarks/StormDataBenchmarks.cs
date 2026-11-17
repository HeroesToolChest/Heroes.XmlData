using BenchmarkDotNet.Attributes;

namespace Heroes.StormData.Benchmarks;

[MemoryDiagnoser]
public class StormDataBenchmarks
{
    [Benchmark]
    public void TestMethod()
    {
        // Your benchmarking code here
    }
}
