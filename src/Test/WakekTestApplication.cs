using System.Collections.ObjectModel;
using System.Threading;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    public class WakekTestApplication : IWakekApplication {
        protected readonly IWakekApplication WrappedWakekApplication;
        public readonly ApplicationCommandController ApplicationCommandController;

        public const string TestBenchmarkGuid = "E87C2B16-8EBD-517F-A5DA-6F915FFD4E60";

        public WakekTestApplication() {
            ApplicationCommandController = new ApplicationCommandController(ApplicationFeedbackHandler);
            WrappedWakekApplication = new WakekApplication(new WakekComponentProvider(new ComponentProvider()), ApplicationCommandController, ApplicationCommandController, SynchronizationContext.Current);
            WrappedWakekApplication.BenchmarkDefinitions.Clear();
            WrappedWakekApplication.BenchmarkDefinitions.Add(new BenchmarkDefinition { BenchmarkExecutionType = BenchmarkExecutionType.CsNative, Description = "Wakek Test Benchmark", ExecutionTimeInSeconds = 2, Guid = TestBenchmarkGuid, NumberOfCallsInParallel = 1 });
        }

        public bool IsExecuting() { return WrappedWakekApplication.IsExecuting(); }
        public IApplicationLog Log { get { return WrappedWakekApplication.Log; } }
        public BenchmarkDefinitions BenchmarkDefinitions { get { return WrappedWakekApplication.BenchmarkDefinitions; } }
        public IBenchmarkDefinition SelectedBenchmarkDefinition { get { return WrappedWakekApplication.SelectedBenchmarkDefinition; } }
        public ObservableCollection<IBenchmarkExecution> BenchmarkExecutions { get { return WrappedWakekApplication.BenchmarkExecutions; } }
        public ObservableCollection<IBenchmarkExecutionState> BenchmarkExecutionStates { get { return WrappedWakekApplication.BenchmarkExecutionStates; } }
        public IWakekComponentProvider WakekComponentProvider { get { return WrappedWakekApplication.WakekComponentProvider; } }

        public void ApplicationFeedbackHandler(IFeedbackToApplication feedback) {
            bool handled;
            ApplicationFeedbackHandler(feedback, out handled);
        }

        public void ApplicationFeedbackHandler(IFeedbackToApplication feedback, out bool handled) {
            WrappedWakekApplication.ApplicationFeedbackHandler(feedback, out handled);
        }

    }
}
