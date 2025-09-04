using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace Heroes.XmlData.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class XmlDataBenchmarks
{
}
