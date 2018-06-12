using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces {
    public interface IBenchmarkDefinition : IGuid {
        string Description { get; }
        string Url { get; }
        string TelemetryUrl { get; }
        BenchmarkExecutionType BenchmarkExecutionType { get; }
        int ExecutionTimeInSeconds { get; }
        int NumberOfCallsInParallel { get; }
    }
}
