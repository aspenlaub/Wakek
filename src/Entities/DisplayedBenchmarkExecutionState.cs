using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;

public class DisplayedBenchmarkExecutionState : IDisplayedBenchmarkExecutionState {
    public string Guid { get; set; }

    public int SequenceNumber { get; set; }
    public string BenchmarkDescription { get; set; }
    public int ExecutingForHowManySeconds { get; set; }
    public int RemoteExecutingForHowManySeconds { get; set; }
    public int RemoteRequiringForHowManySeconds { get; set; }
    public int Successes { get; set; }
    public int Failures { get; set; }
    public bool Finished { get; set; }
}