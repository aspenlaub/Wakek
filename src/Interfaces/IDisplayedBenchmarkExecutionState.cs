using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

public interface IDisplayedBenchmarkExecutionState : IGuid {
    int SequenceNumber { get; }
    string BenchmarkDescription { get; set; }
    int ExecutingForHowManySeconds { get; }
    int RemoteExecutingForHowManySeconds { get; }
    int RemoteRequiringForHowManySeconds { get; }
    int Successes { get; }
    int Failures { get; }
    bool Finished { get; }
}