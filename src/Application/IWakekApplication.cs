using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application;

public interface IWakekApplication : IExecuteCommandContext {
    ISimpleLogger SimpleLogger { get; }
    BenchmarkDefinitions BenchmarkDefinitions { get; }
    ObservableCollection<IBenchmarkExecution> BenchmarkExecutions { get; }
    ObservableCollection<IBenchmarkExecutionState> BenchmarkExecutionStates { get; }
    ObservableCollection<IDisplayedBenchmarkExecutionState> DisplayedBenchmarkExecutionStates { get; }
    Task<bool> HandleFeedbackToApplicationReturnSuccessAsync(IFeedbackToApplication feedback);
    void SelectBenchmarkDefinition(IBenchmarkDefinition benchmarkDefinition);
    IList<T> GetObservableCollectionSnapshot<T>(Func<T, bool> criteria, Func<IList<T>> getObservableCollection);
    Task SetBenchmarkDefinitionsAsync();
}