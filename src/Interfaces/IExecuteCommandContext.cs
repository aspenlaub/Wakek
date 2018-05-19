namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces {
    public interface IExecuteCommandContext {
        bool IsExecuting();
        int NewSequenceNumber();
        IBenchmarkDefinition SelectedBenchmarkDefinition { get; }
    }
}
