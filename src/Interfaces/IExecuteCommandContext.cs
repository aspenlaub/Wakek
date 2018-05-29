using System;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces {
    public interface IExecuteCommandContext {
        bool IsExecuting();
        IWakekComponentProvider WakekComponentProvider { get; }
        IBenchmarkDefinition SelectedBenchmarkDefinition { get; }
        Func<string, int> NavigateToStringReturnContentAsNumber { get; }
    }
}
