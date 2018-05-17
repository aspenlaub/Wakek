namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces {
    public interface IBenchmarkExecutionState {
        int SequenceNumber { get; }
        string BenchmarkExecutionGuid { get; }
        int ExecutingForHowManySeconds { get; }
        int RemoteExecutingForHowManySeconds { get; }
        int RemoteRequiringForHowManySeconds { get; }
        int Successes { get; }
        int Failures { get; }
        bool Finished { get; }
    }
}
