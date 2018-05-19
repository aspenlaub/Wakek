using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Entities.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Basic.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application {
    public class WakekApplication : IWakekApplication {
        protected IComponentProvider ComponentProvider;
        protected IApplicationCommandController Controller;
        protected IApplicationCommandExecutionContext Context;
        protected SynchronizationContext UiSynchronizationContext;
        public IApplicationLog Log { get; }

        public BenchmarkDefinitions BenchmarkDefinitions { get; }
        public IBenchmarkDefinition SelectedBenchmarkDefinition { get; }
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

            BenchmarkExecutions = new ObservableCollection<IBenchmarkExecution>();
            BenchmarkExecutionStates = new ObservableCollection<IBenchmarkExecutionState>();

            Controller.AddCommand(new ExecuteCommand(this), true);
        }

        public bool IsExecuting() {
            return BenchmarkExecutionStates.Any(s => !s.Finished);
        }

        public void ApplicationFeedbackHandler(IFeedbackToApplication feedback, out bool handled) {
            handled = true;
            switch (feedback.Type) {
                case FeedbackType.LogInformation: {
                    Log.Add(new LogEntry { Message = feedback.Message, CreatedAt = feedback.CreatedAt, SequenceNumber = feedback.SequenceNumber });
                } break;
                case FeedbackType.LogWarning: {
                    Log.Add(new LogEntry { Class = LogEntryClass.Warning, Message = feedback.Message, CreatedAt = feedback.CreatedAt, SequenceNumber = feedback.SequenceNumber });
                } break;
                case FeedbackType.LogError: {
                    Log.Add(new LogEntry { Class = LogEntryClass.Error, Message = feedback.Message, CreatedAt = feedback.CreatedAt, SequenceNumber = feedback.SequenceNumber });
                } break;
                case FeedbackType.CommandIsDisabled: {
                    Log.Add(new LogEntry { Class = LogEntryClass.Error, Message = "Attempt to run disabled command " + feedback.CommandType, CreatedAt = feedback.CreatedAt, SequenceNumber = feedback.SequenceNumber });
                } break;
                default: {
                    handled = false;
                } break;
            }
        }
    }
}
