using System;
using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

public interface IExecuteCommandContext {
    bool IsExecuting();
    IBenchmarkDefinition SelectedBenchmarkDefinition { get; }
    Func<string, Task<int>> NavigateToStringReturnContentAsNumberAsync { get; }
}