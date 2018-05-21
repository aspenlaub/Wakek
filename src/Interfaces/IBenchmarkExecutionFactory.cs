namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces {
    public interface IBenchmarkExecutionFactory {
        IBenchmarkExecution CreateBenchmarkExecution(IBenchmarkDefinition benchmarkDefinition, int threadNumber);
        IBenchmarkExecutionState CreateBenchmarkExecutionState(IBenchmarkExecution benchmarkExecution);
    }
}
