namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces {
    public interface IBenchmarkExecutionFactory {
        IBenchmarkExecution CreateBenchmarkExecution(IBenchmarkDefinition benchmarkDefinition);
        IBenchmarkExecutionState CreateBenchmarkExecutionState(IBenchmarkExecution benchmarkExecution, int threadNumber);
    }
}
