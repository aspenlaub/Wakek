using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application {
    public class WakekApplication : IExecute {
        protected IComponentProvider ComponentProvider;
        protected IApplicationCommandController Controller;
        protected IApplicationCommandExecutionContext Context;
        protected SynchronizationContext UiSynchronizationContext;
        public IApplicationLog Log { get; }

        public BenchmarkDefinitions BenchmarkDefinitions { get; }
        public ObservableCollection<BenchmarkDefinition> ObservableBenchmarkDefinitions { get; }
        public BenchmarkDefinition SelectedBenchmarkDefinition { get; }
        public ObservableCollection<IBenchmarkExecution> BenchmarkExecutions { get; }
        public ObservableCollection<IBenchmarkExecutionState> BenchmarkExecutionStates { get; }

        public WakekApplication(IComponentProvider componentProvider, IApplicationCommandController controller, IApplicationCommandExecutionContext context, SynchronizationContext uiSynchronizationContext) {
            ComponentProvider = componentProvider;
            Controller = controller;
            Context = context;
            UiSynchronizationContext = uiSynchronizationContext;
            Log = new ApplicationLog();

            var secret = new SecretBenchmarkDefinitions();
            var errorsAndInfos = new ErrorsAndInfos();
            BenchmarkDefinitions = ComponentProvider.SecretRepository.Get(secret, errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) {
                throw new Exception(string.Join("\r\n", errorsAndInfos.Errors));
            }

            SelectedBenchmarkDefinition = BenchmarkDefinitions[0];

            ObservableBenchmarkDefinitions = new ObservableCollection<BenchmarkDefinition>(BenchmarkDefinitions);
            BenchmarkExecutions = new ObservableCollection<IBenchmarkExecution>();
            BenchmarkExecutionStates = new ObservableCollection<IBenchmarkExecutionState>();

            Controller.AddCommand(new ExecuteCommand(this), true);
        }

        public bool IsExecuting() {
            return BenchmarkExecutionStates.Any(s => !s.Finished);
        }

        public void Execute() {
            throw new NotImplementedException();
        }
    }
}
