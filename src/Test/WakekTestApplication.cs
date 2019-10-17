using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Autofac;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    public class WakekTestApplication : IWakekApplication {
        protected readonly IWakekApplication WrappedWakekApplication;
        public readonly ApplicationCommandController ApplicationCommandController;

        public const string TestBenchmarkGuid = "E87C2B16-8EBD-517F-A5DA-6F915FFD4E60";
        public const string TestParallelBenchmarkGuid = "F9B96A24-45B7-A4BA-32F1-B15D244EA141";

        public WakekTestApplication(IHttpClientFactory httpClientFactory) {
            var container = new ContainerBuilder().UseWakek().Build();
            // var componentProviderMock = new Mock<IWakekComponentProvider>();
            var telemetryDataReaderMock = new Mock<ITelemetryDataReader>();
            IList<ITelemetryData> result = new List<ITelemetryData> {
                new TelemetryData { ExecutingForHowManyMilliSeconds = 24, RequiringForHowManyMilliSeconds = 7 }
            };
            telemetryDataReaderMock.Setup(t => t.ReadAsync(It.IsAny<IBenchmarkDefinition>())).Returns(Task.FromResult(result));
            ApplicationCommandController = new ApplicationCommandController(ApplicationFeedbackHandler);
            WrappedWakekApplication = new WakekApplication(ApplicationCommandController, ApplicationCommandController, SynchronizationContext.Current, NavigateToStringReturnContentAsNumber,
                container.Resolve<ISecretRepository>(), container.Resolve<IXmlSerializedObjectReader>(), container.Resolve<IBenchmarkExecutionFactory>(),
                container.Resolve<IXmlSerializer>(), telemetryDataReaderMock.Object, httpClientFactory);
            WrappedWakekApplication.BenchmarkDefinitions.Clear();
            WrappedWakekApplication.BenchmarkDefinitions.Add(
                new BenchmarkDefinition { BenchmarkExecutionType = BenchmarkExecutionType.CsNative, Description = "Wakek Test Benchmark", ExecutionTimeInSeconds = 2, Guid = TestBenchmarkGuid, NumberOfCallsInParallel = 1 }
            );
            WrappedWakekApplication.BenchmarkDefinitions.Add(
                new BenchmarkDefinition { BenchmarkExecutionType = BenchmarkExecutionType.CsNative, Description = "Wakek Test Parallel Benchmark", ExecutionTimeInSeconds = 2, Guid = TestParallelBenchmarkGuid, NumberOfCallsInParallel = 2 }
            );
            WrappedWakekApplication.BenchmarkDefinitions.Add(
                new BenchmarkDefinition { BenchmarkExecutionType = BenchmarkExecutionType.CsNative, Description = "Wakek Test Benchmark With Url", ExecutionTimeInSeconds = 0, Guid = Guid.NewGuid().ToString(), NumberOfCallsInParallel = 1, Url = @"https://www.viperfisch.de/wakek/helloworld.php", TelemetryUrl = "" }
            );
            WrappedWakekApplication.BenchmarkDefinitions.Add(
                new BenchmarkDefinition { BenchmarkExecutionType = BenchmarkExecutionType.JavaScript, Description = "Wakek Test JavaScript With Url", ExecutionTimeInSeconds = 0, Guid = Guid.NewGuid().ToString(), NumberOfCallsInParallel = 1, Url = @"https://www.viperfisch.de/wakek/helloworld.php", TelemetryUrl = "" }
            );
        }

        public bool IsExecuting() { return WrappedWakekApplication.IsExecuting(); }
        public IApplicationLog Log => WrappedWakekApplication.Log;
        public BenchmarkDefinitions BenchmarkDefinitions => WrappedWakekApplication.BenchmarkDefinitions;
        public IBenchmarkDefinition SelectedBenchmarkDefinition => WrappedWakekApplication.SelectedBenchmarkDefinition;
        public ObservableCollection<IBenchmarkExecution> BenchmarkExecutions => WrappedWakekApplication.BenchmarkExecutions;
        public ObservableCollection<IBenchmarkExecutionState> BenchmarkExecutionStates => WrappedWakekApplication.BenchmarkExecutionStates;
        public ObservableCollection<IDisplayedBenchmarkExecutionState> DisplayedBenchmarkExecutionStates => WrappedWakekApplication.DisplayedBenchmarkExecutionStates;

        public void ApplicationFeedbackHandler(IFeedbackToApplication feedback) {
            ApplicationFeedbackHandler(feedback, out _);
        }

        public void ApplicationFeedbackHandler(IFeedbackToApplication feedback, out bool handled) {
            WrappedWakekApplication.ApplicationFeedbackHandler(feedback, out handled);
        }

        public void SelectBenchmarkDefinition(IBenchmarkDefinition benchmarkDefinition) {
            WrappedWakekApplication.SelectBenchmarkDefinition(benchmarkDefinition);
        }

        public IList<T> GetObservableCollectionSnapshot<T>(Func<T, bool> criteria, Func<IList<T>> getObservableCollection) {
            return WrappedWakekApplication.GetObservableCollectionSnapshot(criteria, getObservableCollection);
        }

        public Func<string, int> NavigateToStringReturnContentAsNumber => html => { return 1; };
    }
}
