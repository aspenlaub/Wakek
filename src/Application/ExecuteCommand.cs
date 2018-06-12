using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task Execute(IApplicationCommandExecutionContext context) {
            var executionStart = DateTime.Now;
            var benchmarkDefinition = ContextOwner.SelectedBenchmarkDefinition;
            var benchmarkExecution = WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecution(benchmarkDefinition) as BenchmarkExecution;
            context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecution) });
            context.Report(new FeedbackToApplication { Type = FeedbackType.LogInformation, Message = string.Format(Properties.Resources.CreatingThreads, benchmarkDefinition.NumberOfCallsInParallel) });

            var initialTelemetryData = await WakekComponentProvider.TelemetryDataReader.ReadAsync(benchmarkDefinition);

            var client = WakekComponentProvider.HttpClient;
            var url = benchmarkDefinition.Url;
            if (!string.IsNullOrEmpty(url)) {
                client.BaseAddress = new Uri(url);
            }

            BenchmarkExecutionState firstBestExecutionState = null;
            switch (benchmarkDefinition.BenchmarkExecutionType) {
                case BenchmarkExecutionType.CsNative: {
                    var tasks = Enumerable.Range(1, benchmarkDefinition.NumberOfCallsInParallel).Select(t => ExecuteForThreadNativeCsAsync(context, benchmarkDefinition, benchmarkExecution, t, executionStart, client));
                    var executionStates = await Task.WhenAll(tasks);
                    firstBestExecutionState = executionStates[0];
                }
                break;
                case BenchmarkExecutionType.JavaScript: {
                    firstBestExecutionState = await ExecuteForThreadJavaScriptAsync(context, benchmarkDefinition, benchmarkExecution, executionStart, client);
                }
                break;
            }

            var finalTelemetryData = await WakekComponentProvider.TelemetryDataReader.ReadAsync(benchmarkDefinition);
            if (firstBestExecutionState != null && finalTelemetryData.Count != 0) {
                firstBestExecutionState.RemoteRequiringForHowManySeconds = SecondsSpent(initialTelemetryData, finalTelemetryData, t => t.RequiringForHowManyMilliSeconds);
                firstBestExecutionState.RemoteExecutingForHowManySeconds = SecondsSpent(initialTelemetryData, finalTelemetryData, t => t.ExecutingForHowManyMilliSeconds);
                context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(firstBestExecutionState) });
            }


            context.Report(new FeedbackToApplication { Type = FeedbackType.LogInformation, Message = Properties.Resources.AllThreadsFinished });
        }

        private int SecondsSpent(IEnumerable<ITelemetryData> initialTelemetryData, IEnumerable<ITelemetryData> finalTelemetryData, Func<ITelemetryData, int> selector) {
            var sumInitial = initialTelemetryData.Sum(selector);
            var sumFinal = finalTelemetryData.Sum(selector);
            return (int)Math.Ceiling(0.001 * (sumFinal - sumInitial));
        }

        private async Task<BenchmarkExecutionState> ExecuteForThreadNativeCsAsync(IApplicationCommandExecutionContext context, IBenchmarkDefinition benchmarkDefinition, IBenchmarkExecution benchmarkExecution, int threadNumber, DateTime executionStart, IHttpClient client) {
            DateTime threadExecutionEnd;
            var benchmarkExecutionState = BeginExecuteForThread(context, benchmarkDefinition, benchmarkExecution, threadNumber, executionStart, out threadExecutionEnd);

            var counter = 0;
            do {
                if (string.IsNullOrEmpty(benchmarkDefinition.Url)) {
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
                else {
                    try {
                        var requestedAt = DateTime.Now;
                        await client.GetStringAsync("?g=" + Guid.NewGuid());
                        var requestDuration = DateTime.Now.Subtract(requestedAt);
                        context.Report(new FeedbackToApplication {
                            Type = FeedbackType.LogInformation,
                            Message = string.Format(Properties.Resources.WebRequestFinished, (int) requestDuration.TotalMilliseconds, threadNumber)
                        });
                        benchmarkExecutionState.Successes++;
                    }
                    catch {
                        benchmarkExecutionState.Failures++;
                    }
                }

                benchmarkExecutionState.ExecutingForHowManySeconds = (int) Math.Floor((DateTime.Now - executionStart).TotalSeconds);

                counter = (counter + 1) % 10;
                if (counter == 1) {
                    context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecutionState) });
                }
            } while (DateTime.Now < threadExecutionEnd);

            EndExecuteForThread(context, threadNumber, executionStart, benchmarkExecutionState);
            return benchmarkExecutionState;
        }

        private async Task<BenchmarkExecutionState> ExecuteForThreadJavaScriptAsync(IApplicationCommandExecutionContext context, IBenchmarkDefinition benchmarkDefinition, IBenchmarkExecution benchmarkExecution, DateTime executionStart, IHttpClient client) {
            var jQueryScript = await client.GetStringAsync("http://code.jquery.com/jquery-2.0.0.min.js");
            var html = @"<!DOCTYPE html><html><head><meta http-equiv='X-UA-Compatible' content='IE=edge;chrome=1' /><script type='text/javascript'>" + jQueryScript + ' '
                + @"$(document).ready(function() { executionEndTime  = new Date(); executionEndTime.setSeconds(executionEndTime.getSeconds() + "
                + benchmarkDefinition.ExecutionTimeInSeconds + @"); successes = 0; function callAsManyTimesAsPossible(x) { "
                + @"$.ajax({ url: '" + benchmarkDefinition.Url + @"', cache: false, async: true }).success(function(text) { "
                + @"successes ++; currentTime = new Date(); if (currentTime <= executionEndTime) { callAsManyTimesAsPossible(x); } else { $(""#content"").html(successes); } "
                + @"}); } ["
                + string.Join(",", Enumerable.Range(1, benchmarkDefinition.NumberOfCallsInParallel))
                + @"].forEach(function(x) { callAsManyTimesAsPossible(x); });
                });
                </script>
                </head><body lang='en'><div id='content'>Please wait..</div></body></html>";

            DateTime threadExecutionEnd;
            var benchmarkExecutionState = BeginExecuteForThread(context, benchmarkDefinition, benchmarkExecution, 1, executionStart, out threadExecutionEnd);

            var task = Task.Factory.StartNew(() => ContextOwner.NavigateToStringReturnContentAsNumber(html));
            return await task.ContinueWith(t => JavaScriptFinished(t, context, executionStart, benchmarkExecutionState));
        }

        private BenchmarkExecutionState JavaScriptFinished(Task<int> task, IApplicationCommandExecutionContext context, DateTime executionStart, BenchmarkExecutionState benchmarkExecutionState) {
            benchmarkExecutionState.Successes = task.Result;

            EndExecuteForThread(context, 1, executionStart, benchmarkExecutionState);
            return benchmarkExecutionState;
        }

        private void EndExecuteForThread(IApplicationCommandExecutionContext context, int threadNumber, DateTime executionStart, BenchmarkExecutionState benchmarkExecutionState) {
            benchmarkExecutionState.ExecutingForHowManySeconds = (int)Math.Floor((DateTime.Now - executionStart).TotalSeconds);
            benchmarkExecutionState.Finished = true;
            context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecutionState) });
            context.Report(new FeedbackToApplication { Type = FeedbackType.LogInformation, Message = string.Format(Properties.Resources.FinishedThread, threadNumber) });
        }

        private BenchmarkExecutionState BeginExecuteForThread(IApplicationCommandExecutionContext context, IBenchmarkDefinition benchmarkDefinition, IBenchmarkExecution benchmarkExecution, int threadNumber, DateTime executionStart,
            out DateTime threadExecutionEnd) {
            var benchmarkExecutionState = WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecutionState(benchmarkExecution, threadNumber) as BenchmarkExecutionState;
            if (benchmarkExecutionState == null) {
                throw new NullReferenceException();
            }

            threadExecutionEnd = executionStart.AddSeconds(benchmarkDefinition.ExecutionTimeInSeconds);

            context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecutionState) });
            context.Report(new FeedbackToApplication { Type = FeedbackType.LogInformation, Message = string.Format(Properties.Resources.CreatedThread, threadNumber) });
            return benchmarkExecutionState;
        }
    }
}
