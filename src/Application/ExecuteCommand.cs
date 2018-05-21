using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application {
    public class ExecuteCommand : IApplicationCommand {
        protected IExecuteCommandContext ContextOwner;
        protected IWakekComponentProvider WakekComponentProvider;
        protected XmlSerializer XmlSerializer;

        public bool MakeLogEntries => true;
        public string Name => Properties.Resources.ExecuteCommandName;

        public ExecuteCommand(IExecuteCommandContext contextOwner) {
            ContextOwner = contextOwner;
            WakekComponentProvider = ContextOwner.WakekComponentProvider;
            XmlSerializer = new XmlSerializer();
        }

        public bool CanExecute() {
            return !ContextOwner.IsExecuting() && !string.IsNullOrEmpty(ContextOwner.SelectedBenchmarkDefinition?.Guid);
        }

        public Task Execute(IApplicationCommandExecutionContext context) {
            var executionStart = DateTime.Now;
            var benchmarkExecution = WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecution(ContextOwner.SelectedBenchmarkDefinition) as BenchmarkExecution;
            context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecution) });
            var tasks = Enumerable.Range(1, ContextOwner.SelectedBenchmarkDefinition.NumberOfCallsInParallel).Select(t => Task.Run(() => ExecuteForThread(context, benchmarkExecution, t, executionStart)));
            return Task.WhenAll(tasks);
        }

        private void ExecuteForThread(IApplicationCommandExecutionContext context, IBenchmarkExecution benchmarkExecution, int threadNumber, DateTime executionStart) {
            var benchmarkExecutionState = WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecutionState(benchmarkExecution, threadNumber) as BenchmarkExecutionState;
            if (benchmarkExecutionState == null) { throw new NullReferenceException(); }

            var threadExecutionEnd = executionStart.AddSeconds(ContextOwner.SelectedBenchmarkDefinition.ExecutionTimeInSeconds);

            context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecutionState) });

            var counter = 0;
            while (DateTime.Now < threadExecutionEnd) {
                if (string.IsNullOrEmpty(ContextOwner.SelectedBenchmarkDefinition.Url)) {
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                } else {
                    var request = (HttpWebRequest)WebRequest.Create(ContextOwner.SelectedBenchmarkDefinition.Url);
                    request.Method = WebRequestMethods.Http.Get;
                    request.UserAgent = GetType().Namespace;
                    request.Timeout = ContextOwner.SelectedBenchmarkDefinition.ExecutionTimeInSeconds * 1000;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    try {
                        using (var response = (HttpWebResponse) request.GetResponse()) {
                            var responseStream = response.GetResponseStream();
                            if (responseStream == null) {
                                benchmarkExecutionState.Failures++;
                            } else {
                                benchmarkExecutionState.Successes++;
                            }
                        }
                    } catch {
                        benchmarkExecutionState.Failures++;
                    }
                }

                benchmarkExecutionState.ExecutingForHowManySeconds = (int)Math.Floor((DateTime.Now - executionStart).TotalSeconds);

                counter = (counter + 1) % 10;
                if (counter != 0) {
                    continue;
                }

                context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecutionState) });
            }

            benchmarkExecutionState.ExecutingForHowManySeconds = (int)Math.Floor((DateTime.Now - executionStart).TotalSeconds);
            benchmarkExecutionState.Finished = true;
            context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecutionState) });
        }
    }
}
