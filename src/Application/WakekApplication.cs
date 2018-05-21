using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application {
    public class WakekApplication : IWakekApplication {
        public IWakekComponentProvider WakekComponentProvider { get; }
        protected IApplicationCommandController Controller;
        protected IApplicationCommandExecutionContext Context;
        protected SynchronizationContext UiSynchronizationContext;
        public IApplicationLog Log { get; }

        public BenchmarkDefinitions BenchmarkDefinitions { get; }
        public IBenchmarkDefinition SelectedBenchmarkDefinition { get; set; }
        public ObservableCollection<IBenchmarkExecution> BenchmarkExecutions { get; }
        public ObservableCollection<IBenchmarkExecutionState> BenchmarkExecutionStates { get; }

        protected int NextSequenceNumber;

        public WakekApplication(IWakekComponentProvider wakekComponentProvider, IApplicationCommandController controller, IApplicationCommandExecutionContext context, SynchronizationContext uiSynchronizationContext) {
            WakekComponentProvider = wakekComponentProvider;
            Controller = controller;
            Context = context;
            UiSynchronizationContext = uiSynchronizationContext;
            Log = new ApplicationLog();
            NextSequenceNumber = 1;

            var secret = new SecretBenchmarkDefinitions();
            var errorsAndInfos = new ErrorsAndInfos();
            BenchmarkDefinitions = WakekComponentProvider.PeghComponentProvider.SecretRepository.Get(secret, errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) {
                throw new Exception(string.Join("\r\n", errorsAndInfos.Errors));
            }

            BenchmarkDefinitions.CollectionChanged += BenchmarkDefinitionsOnCollectionChanged;

            SelectedBenchmarkDefinition = BenchmarkDefinitions[0];

            BenchmarkExecutions = new ObservableCollection<IBenchmarkExecution>();
            BenchmarkExecutionStates = new ObservableCollection<IBenchmarkExecutionState>();

            Controller.AddCommand(new ExecuteCommand(this), true);
        }

        private void BenchmarkDefinitionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            if (SelectedBenchmarkDefinition == null) {
                if (!BenchmarkDefinitions.Any()) { return; }

                SelectedBenchmarkDefinition = BenchmarkDefinitions[0];
                return;
            }

            if (BenchmarkDefinitions.Any(b => b.Guid == SelectedBenchmarkDefinition.Guid)) { return; }

            SelectedBenchmarkDefinition = null;
        }

        public bool IsExecuting() {
            return BenchmarkExecutionStates.Any(s => !s.Finished);
        }

        public void ApplicationFeedbackHandler(IFeedbackToApplication feedback, out bool handled) {
            handled = true;
            switch (feedback.Type) {
                case FeedbackType.ImportantMessage: {
                    Type feedbackSerializedObjectType;
                    WakekComponentProvider.XmlSerializedObjectReader.IdentifyType(feedback.Message, out handled, out feedbackSerializedObjectType);
                    if (!handled) { return; }

                    if (feedbackSerializedObjectType == typeof(BenchmarkExecution)) {
                        var benchmarkExecution = WakekComponentProvider.XmlSerializedObjectReader.Read<BenchmarkExecution>(feedback.Message);
                        ReplaceOrAddToCollection(benchmarkExecution, BenchmarkExecutions);
                    } else if (feedbackSerializedObjectType == typeof(BenchmarkExecutionState)) {
                        var benchmarkExecutionState = WakekComponentProvider.XmlSerializedObjectReader.Read<BenchmarkExecutionState>(feedback.Message);
                        ReplaceOrAddToCollection(benchmarkExecutionState, BenchmarkExecutionStates);
                    } else {
                        handled = false;
                    }
                // ReSharper disable once SeparateControlTransferStatement
                } break;
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

        private static void ReplaceOrAddToCollection<T>(T item, IList<T> collection) where T : IGuid {
            if (string.IsNullOrEmpty(item.Guid)) { return; }

            for (var i = 0; i < collection.Count; i++) {
                if (collection[i].Guid != item.Guid) {
                    continue;
                }

                collection[i] = item;
                return;
            }

            collection.Add(item);
        }
    }
}
