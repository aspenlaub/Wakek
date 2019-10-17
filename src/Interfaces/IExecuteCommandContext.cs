using System;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces {
    public interface IExecuteCommandContext {
        bool IsExecuting();
        IBenchmarkDefinition SelectedBenchmarkDefinition { get; }
        Func<string, int> NavigateToStringReturnContentAsNumber { get; }
    }
}
