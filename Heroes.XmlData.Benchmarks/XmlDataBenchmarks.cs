using BenchmarkDotNet.Attributes;

namespace Heroes.XmlData.Benchmarks;

[MemoryDiagnoser]
public class XmlDataBenchmarks
{
    [Benchmark]
    public void TestMethod()
    {
        // Your benchmarking code here
    }
}
