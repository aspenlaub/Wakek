using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application {
    public interface IWakekApplication : IExecuteCommandContext {
        IApplicationLog Log { get; }
        BenchmarkDefinitions BenchmarkDefinitions { get; }
        ObservableCollection<IBenchmarkExecution> BenchmarkExecutions { get; }
        ObservableCollection<IBenchmarkExecutionState> BenchmarkExecutionStates { get; }
        ObservableCollection<IDisplayedBenchmarkExecutionState> DisplayedBenchmarkExecutionStates { get; }
        void ApplicationFeedbackHandler(IFeedbackToApplication feedback, out bool handled);
        void SelectBenchmarkDefinition(IBenchmarkDefinition benchmarkDefinition);
        IList<T> GetObservableCollectionSnapshot<T>(Func<T, bool> criteria, Func<IList<T>> getObservableCollection);
    }
}