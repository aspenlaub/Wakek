using System.Collections.ObjectModel;
using System.Threading;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    public class WakekTestApplication : IWakekApplication {
        protected readonly IWakekApplication WrappedWakekApplication;

        public WakekTestApplication() {
            var commandControllerMock = new Mock<IApplicationCommandController>();
            var commandExecutionContextMock = new Mock<IApplicationCommandExecutionContext>();
            WrappedWakekApplication = new WakekApplication(new ComponentProvider(), commandControllerMock.Object, commandExecutionContextMock.Object, SynchronizationContext.Current);
        }

        public bool IsExecuting() {
            throw new System.NotImplementedException();
        }

        public IApplicationLog Log { get { return WrappedWakekApplication.Log; } }
        public BenchmarkDefinitions BenchmarkDefinitions { get { return WrappedWakekApplication.BenchmarkDefinitions; } }
        public IBenchmarkDefinition SelectedBenchmarkDefinition { get { return WrappedWakekApplication.SelectedBenchmarkDefinition; } }
        public ObservableCollection<IBenchmarkExecution> BenchmarkExecutions { get { return WrappedWakekApplication.BenchmarkExecutions; } }
        public ObservableCollection<IBenchmarkExecutionState> BenchmarkExecutionStates { get { return WrappedWakekApplication.BenchmarkExecutionStates; } }
        public void ApplicationFeedbackHandler(IFeedbackToApplication feedback, out bool handled) {
            WrappedWakekApplication.ApplicationFeedbackHandler(feedback, out handled);
        }
    }
}
