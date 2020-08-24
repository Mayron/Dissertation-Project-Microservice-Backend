using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace OpenSpark.StressTests
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class CreateProjectSagaBenchmarks
    {
        [Benchmark]
        public void ExecuteSaga()
        {
        }
    }
}