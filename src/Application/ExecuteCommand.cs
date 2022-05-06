using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application {
    public class ExecuteCommand : IApplicationCommand {
        protected IExecuteCommandContext ContextOwner;

        public bool MakeLogEntries => true;
        public string Name => Properties.Resources.ExecuteCommandName;

        private readonly IBenchmarkExecutionFactory BenchmarkExecutionFactory;
        private readonly IXmlSerializer XmlSerializer;
        private readonly ITelemetryDataReader TelemetryDataReader;
        private readonly IHttpClientFactory HttpClientFactory;

        public ExecuteCommand(IExecuteCommandContext contextOwner, IBenchmarkExecutionFactory benchmarkExecutionFactory, IXmlSerializer xmlSerializer,
                ITelemetryDataReader telemetryDataReader, IHttpClientFactory httpClientFactory) {
            ContextOwner = contextOwner;
            XmlSerializer = xmlSerializer;
            BenchmarkExecutionFactory = benchmarkExecutionFactory;
            TelemetryDataReader = telemetryDataReader;
            HttpClientFactory = httpClientFactory;
        }

        public async Task<bool> CanExecuteAsync() {
            return await Task.FromResult(!ContextOwner.IsExecuting() && !string.IsNullOrEmpty(ContextOwner.SelectedBenchmarkDefinition?.Guid));
        }

        public async Task ExecuteAsync(IApplicationCommandExecutionContext context) {
            var executionStart = DateTime.Now;
            var benchmarkDefinition = ContextOwner.SelectedBenchmarkDefinition;
            var benchmarkExecution = BenchmarkExecutionFactory.CreateBenchmarkExecution(benchmarkDefinition) as BenchmarkExecution;
            await context.ReportAsync(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecution) });
            await context.ReportAsync(new FeedbackToApplication { Type = FeedbackType.LogInformation, Message = string.Format(Properties.Resources.CreatingThreads, benchmarkDefinition.NumberOfCallsInParallel) });

            var initialTelemetryData = await TelemetryDataReader.ReadAsync(benchmarkDefinition);

            using (var client = HttpClientFactory.Create()) {
                var url = benchmarkDefinition.Url;
                if (!string.IsNullOrEmpty(url)) {
                    client.BaseAddress = new Uri(url);
                }

                BenchmarkExecutionState firstBestExecutionState = null;
                switch (benchmarkDefinition.BenchmarkExecutionType) {
                    case BenchmarkExecutionType.CsNative: {
                        var clientCopy = client;
                        var tasks = Enumerable.Range(1, benchmarkDefinition.NumberOfCallsInParallel).Select(t => ExecuteForThreadNativeCsAsync(context, benchmarkDefinition, benchmarkExecution, t, executionStart, clientCopy));
                        var executionStates = await Task.WhenAll(tasks);
                        firstBestExecutionState = executionStates[0];
                    }
                    break;
                    case BenchmarkExecutionType.JavaScript: {
                        firstBestExecutionState = await ExecuteForThreadJavaScriptAsync(context, benchmarkDefinition, benchmarkExecution, executionStart, client);
                    }
                    break;
                }
                var finalTelemetryData = await TelemetryDataReader.ReadAsync(benchmarkDefinition);
                if (firstBestExecutionState != null && finalTelemetryData.Count != 0) {
                    firstBestExecutionState.RemoteRequiringForHowManySeconds = SecondsSpent(initialTelemetryData, finalTelemetryData, t => t.RequiringForHowManyMilliSeconds);
                    firstBestExecutionState.RemoteExecutingForHowManySeconds = SecondsSpent(initialTelemetryData, finalTelemetryData, t => t.ExecutingForHowManyMilliSeconds);
                    await context.ReportAsync(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(firstBestExecutionState) });
                }
            }


            await context.ReportAsync(new FeedbackToApplication { Type = FeedbackType.LogInformation, Message = Properties.Resources.AllThreadsFinished });
        }

        private int SecondsSpent(IEnumerable<ITelemetryData> initialTelemetryData, IEnumerable<ITelemetryData> finalTelemetryData, Func<ITelemetryData, int> selector) {
            var sumInitial = initialTelemetryData.Sum(selector);
            var sumFinal = finalTelemetryData.Sum(selector);
            return (int)Math.Ceiling(0.001 * (sumFinal - sumInitial));
        }

        private async Task<BenchmarkExecutionState> ExecuteForThreadNativeCsAsync(IApplicationCommandExecutionContext context, IBenchmarkDefinition benchmarkDefinition, IBenchmarkExecution benchmarkExecution, int threadNumber, DateTime executionStart, IHttpClient client) {
            var result = await BeginExecuteForThreadAsync(context, benchmarkDefinition, benchmarkExecution, threadNumber, executionStart);
            var benchmarkExecutionState = result.BenchmarkExecutionState;
            var threadExecutionEnd = result.ThreadExecutionEnd;

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
                        await context.ReportAsync(new FeedbackToApplication {
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
                    await context.ReportAsync(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecutionState) });
                }
            } while (DateTime.Now < threadExecutionEnd);

            await EndExecuteForThreadAsync(context, threadNumber, executionStart, benchmarkExecutionState);
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

            var benchmarkExecutionState = (await BeginExecuteForThreadAsync(context, benchmarkDefinition, benchmarkExecution, 1, executionStart)).BenchmarkExecutionState;

            // var task = Task.Factory.StartNew(() => ContextOwner.NavigateToStringReturnContentAsNumber(html));
            // return await task.ContinueWith(t => JavaScriptFinishedAsync(t, context, executionStart, benchmarkExecutionState));
            return await JavaScriptFinishedAsync(await ContextOwner.NavigateToStringReturnContentAsNumberAsync(html), context, executionStart, benchmarkExecutionState);
        }

        private async Task<BenchmarkExecutionState> JavaScriptFinishedAsync(int contentAsNumber, IApplicationCommandExecutionContext context, DateTime executionStart, BenchmarkExecutionState benchmarkExecutionState) {
            benchmarkExecutionState.Successes = contentAsNumber;

            await EndExecuteForThreadAsync(context, 1, executionStart, benchmarkExecutionState);
            return benchmarkExecutionState;
        }

        private async Task EndExecuteForThreadAsync(IApplicationCommandExecutionContext context, int threadNumber, DateTime executionStart, BenchmarkExecutionState benchmarkExecutionState) {
            benchmarkExecutionState.ExecutingForHowManySeconds = (int)Math.Floor((DateTime.Now - executionStart).TotalSeconds);
            benchmarkExecutionState.Finished = true;
            await context.ReportAsync(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecutionState) });
            await context.ReportAsync(new FeedbackToApplication { Type = FeedbackType.LogInformation, Message = string.Format(Properties.Resources.FinishedThread, threadNumber) });
        }

        private async Task<ExtendedBenchmarkExecutionState> BeginExecuteForThreadAsync(IApplicationCommandExecutionContext context, IBenchmarkDefinition benchmarkDefinition, IBenchmarkExecution benchmarkExecution, int threadNumber, DateTime executionStart) {
            var result = new ExtendedBenchmarkExecutionState {
                BenchmarkExecutionState = BenchmarkExecutionFactory.CreateBenchmarkExecutionState(benchmarkExecution, threadNumber) as BenchmarkExecutionState
            };
            if (result.BenchmarkExecutionState == null) {
                throw new NullReferenceException();
            }

            result.ThreadExecutionEnd = executionStart.AddSeconds(benchmarkDefinition.ExecutionTimeInSeconds);

            await context.ReportAsync(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(result.BenchmarkExecutionState) });
            await context.ReportAsync(new FeedbackToApplication { Type = FeedbackType.LogInformation, Message = string.Format(Properties.Resources.CreatedThread, threadNumber) });
            return result;
        }
    }

    internal class ExtendedBenchmarkExecutionState {
        public BenchmarkExecutionState BenchmarkExecutionState { get; set; }
        public DateTime ThreadExecutionEnd { get; set; }
    }
}
