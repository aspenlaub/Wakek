using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        protected IApplicationCommandController Controller;
        protected IApplicationCommandExecutionContext Context;
        protected SynchronizationContext UiSynchronizationContext;
        protected ISecretRepository SecretRepository;
        public Func<string, Task<int>> NavigateToStringReturnContentAsNumberAsync { get; }
        public IApplicationLog Log { get; }

        public BenchmarkDefinitions BenchmarkDefinitions { get; private set; }
        public IBenchmarkDefinition SelectedBenchmarkDefinition { get; private set; }
        public ObservableCollection<IBenchmarkExecution> BenchmarkExecutions { get; private set; }
        public ObservableCollection<IBenchmarkExecutionState> BenchmarkExecutionStates { get; private set; }
        public ObservableCollection<IDisplayedBenchmarkExecutionState> DisplayedBenchmarkExecutionStates { get; private set; }

        protected static object LockObject = new();

        private readonly IXmlSerializedObjectReader XmlSerializedObjectReader;

        protected int NextSequenceNumber;

        public WakekApplication(IApplicationCommandController controller, IApplicationCommandExecutionContext context, SynchronizationContext uiSynchronizationContext, Func<string, Task<int>> navigateToStringReturnContentAsNumberAsync,
                ISecretRepository secretRepository, IXmlSerializedObjectReader xmlSerializedObjectReader, IBenchmarkExecutionFactory benchmarkExecutionFactory,
                IXmlSerializer xmlSerializer, ITelemetryDataReader telemetryDataReader, IHttpClientFactory httpClientFactory) {
            Controller = controller;
            Context = context;
            UiSynchronizationContext = uiSynchronizationContext;
            NavigateToStringReturnContentAsNumberAsync = navigateToStringReturnContentAsNumberAsync;
            XmlSerializedObjectReader = xmlSerializedObjectReader;
            Log = new ApplicationLog();
            NextSequenceNumber = 1;
            SecretRepository = secretRepository;
            Controller.AddCommand(new ExecuteCommand(this, benchmarkExecutionFactory, xmlSerializer, telemetryDataReader, httpClientFactory), true);
        }

        public async Task SetBenchmarkDefinitionsAsync() {
            var secret = new SecretBenchmarkDefinitions();
            var errorsAndInfos = new ErrorsAndInfos();
            BenchmarkDefinitions = await SecretRepository.GetAsync(secret, errorsAndInfos);
            if (errorsAndInfos.AnyErrors()) {
                throw new Exception(string.Join("\r\n", errorsAndInfos.Errors));
            }

            SelectedBenchmarkDefinition = BenchmarkDefinitions[0];
            BenchmarkExecutions = new ObservableCollection<IBenchmarkExecution>();
            BenchmarkExecutionStates = new ObservableCollection<IBenchmarkExecutionState>();
            DisplayedBenchmarkExecutionStates = new ObservableCollection<IDisplayedBenchmarkExecutionState>();

            BenchmarkDefinitions.CollectionChanged += BenchmarkDefinitionsOnCollectionChanged;
            BenchmarkExecutionStates.CollectionChanged += BenchmarkExecutionStatesOnCollectionChanged;
        }

        private void BenchmarkExecutionStatesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs) {
            // ReSharper disable once LoopCanBePartlyConvertedToQuery
            foreach (var execution in BenchmarkExecutions) {
                var definition = BenchmarkDefinitions.FirstOrDefault(d => d.Guid == execution.BenchmarkDefinitionGuid);
                if (definition == null) { continue; }

                var states = GetObservableCollectionSnapshot(s => s.BenchmarkExecutionGuid == execution.Guid, () => BenchmarkExecutionStates);
                if (!states.Any()) { continue; }

                var displayedState = new DisplayedBenchmarkExecutionState {
                    BenchmarkDescription = definition.Description,
                    ExecutingForHowManySeconds = states.Max(s => s.ExecutingForHowManySeconds),
                    Failures = states.Sum(s => s.Failures),
                    Finished = states.All(s => s.Finished),
                    Guid = execution.Guid,
                    RemoteExecutingForHowManySeconds = states.Sum(s => s.RemoteExecutingForHowManySeconds),
                    RemoteRequiringForHowManySeconds = states.Sum(s => s.RemoteRequiringForHowManySeconds),
                    SequenceNumber = states.Min(s => s.SequenceNumber),
                    Successes = states.Sum(s => s.Successes)
                };

                ReplaceOrAddToCollection(displayedState, DisplayedBenchmarkExecutionStates);
            }
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
            lock(LockObject) {
                return BenchmarkExecutionStates.Any(s => !s.Finished);
            }
        }

        public async Task HandleFeedbackToApplicationAsync(IFeedbackToApplication feedback) {
            await HandleFeedbackToApplicationReturnSuccessAsync(feedback);
        }

        public async Task<bool> HandleFeedbackToApplicationReturnSuccessAsync(IFeedbackToApplication feedback) {
            var handled = true;
            switch (feedback.Type) {
                case FeedbackType.ImportantMessage: {
                    XmlSerializedObjectReader.IdentifyType(feedback.Message, out handled, out var feedbackSerializedObjectType);
                    if (!handled) { return false; }

                    if (feedbackSerializedObjectType == typeof(BenchmarkExecution)) {
                        var benchmarkExecution = XmlSerializedObjectReader.Read<BenchmarkExecution>(feedback.Message);
                        ReplaceOrAddToCollection(benchmarkExecution, BenchmarkExecutions);
                    } else if (feedbackSerializedObjectType == typeof(BenchmarkExecutionState)) {
                        var benchmarkExecutionState = XmlSerializedObjectReader.Read<BenchmarkExecutionState>(feedback.Message);
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

            return await Task.FromResult(handled);
        }

        private static void ReplaceOrAddToCollection<T>(T item, IList<T> collection) where T : IGuid {
            if (string.IsNullOrEmpty(item.Guid)) {
                throw new NullReferenceException("item.Guid");
            }

            lock (LockObject) {
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

        public void SelectBenchmarkDefinition(IBenchmarkDefinition benchmarkDefinition) {
            SelectedBenchmarkDefinition = benchmarkDefinition;
        }

        public IList<T> GetObservableCollectionSnapshot<T>(Func<T, bool> criteria, Func<IList<T>> getObservableCollection) {
            lock (LockObject) {
                return new List<T>(getObservableCollection().Where(x => criteria(x)));
            }
        }
    }
}
