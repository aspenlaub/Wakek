using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class ExecuteCommandTest {
        protected WakekTestApplication WakekApplication;
        protected ExecuteCommand ExecuteCommand;
        protected IApplicationCommandExecutionContext ApplicationCommandExecutionContext;
        protected bool HttpClientGetStringSucceeds;

        [TestInitialize]
        public void Initialize() {
            HttpClientGetStringSucceeds = true;
            var httpClientMock = new Mock<IHttpClient>();
            httpClientMock.SetupGet(h => h.BaseAddress);
            httpClientMock.Setup(h => h.GetStringAsync(It.IsAny<string>())).Returns<string>(s => HttpClientGetStringResult());
            WakekApplication = new WakekTestApplication(httpClientMock.Object);
            ExecuteCommand = new ExecuteCommand(WakekApplication);
            var applicationCommandExecutionContextMock = new Mock<IApplicationCommandExecutionContext>();
            applicationCommandExecutionContextMock.Setup(a => a.Report(It.IsAny<IFeedbackToApplication>())).Callback<IFeedbackToApplication>(f => WakekApplication.ApplicationFeedbackHandler(f));
            ApplicationCommandExecutionContext = applicationCommandExecutionContextMock.Object;
        }

        private Task<string> HttpClientGetStringResult() {
            if (HttpClientGetStringSucceeds) { return Task.FromResult("Hello World"); }

            throw new Exception("Http call failed");
        }

        [TestMethod]
        public async Task ExecuteCommandCreatesExecutionAndState() {
            Assert.IsNotNull(WakekApplication);
            Assert.AreEqual(0, WakekApplication.BenchmarkExecutions.Count);
            await WakekApplication.ApplicationCommandController.Execute(typeof(ExecuteCommand));
            Assert.AreEqual(1, WakekApplication.BenchmarkExecutions.Count);
            var executionGuid = WakekApplication.BenchmarkExecutions[0].Guid;
            var states = await GetStatesForExecution(executionGuid, 1);
            var state = states[0] as BenchmarkExecutionState;
            Assert.IsNotNull(state);
            var displayedStates = await GetDisplayedStatesForExecution(1);
            var displayedState = displayedStates[0];
            Assert.AreEqual(WakekApplication.SelectedBenchmarkDefinition.Description, displayedState.BenchmarkDescription);
            Assert.AreEqual(state.ExecutingForHowManySeconds, displayedState.ExecutingForHowManySeconds);
            Assert.AreEqual(state.Failures, displayedState.Failures);
            Assert.AreEqual(state.Successes, displayedState.Successes);
            Assert.AreEqual(state.RemoteExecutingForHowManySeconds, displayedState.RemoteExecutingForHowManySeconds);
            Assert.AreEqual(state.RemoteRequiringForHowManySeconds, displayedState.RemoteRequiringForHowManySeconds);
            Assert.AreEqual(state.Finished, displayedState.Finished);

            bool handled;
            var feedback = new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = WakekApplication.WakekComponentProvider.PeghComponentProvider.XmlSerializer.Serialize(state) };
            WakekApplication.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
            await GetStatesForExecution(executionGuid, 1);

            var execution = WakekApplication.WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecution(WakekApplication.BenchmarkDefinitions[0]);
            state = WakekApplication.WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecutionState(execution, 1) as BenchmarkExecutionState;
            feedback = new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = WakekApplication.WakekComponentProvider.PeghComponentProvider.XmlSerializer.Serialize(state) };
            WakekApplication.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
            await GetStatesForExecution(2);
        }

        [TestMethod]
        public async Task ExecuteCommandCreatesExecutionAndStateForMultiThreadBenchmarks() {
            Assert.IsNotNull(WakekApplication);
            Assert.AreEqual(0, WakekApplication.BenchmarkExecutions.Count);
            WakekApplication.SelectBenchmarkDefinition(WakekApplication.BenchmarkDefinitions[1]);
            await WakekApplication.ApplicationCommandController.Execute(typeof(ExecuteCommand));
            Assert.AreEqual(1, WakekApplication.BenchmarkExecutions.Count);
            var executionGuid = WakekApplication.BenchmarkExecutions[0].Guid;

            var states = await GetStatesForExecution(executionGuid, 2);
            Assert.IsTrue(states.Any(s => s.ThreadNumber == 1));
            Assert.IsTrue(states.Any(s => s.ThreadNumber == 2));

            var displayedStates = await GetDisplayedStatesForExecution(1);
            Assert.AreEqual(WakekApplication.SelectedBenchmarkDefinition.Description, displayedStates[0].BenchmarkDescription);
        }

        private async Task<IList<IBenchmarkExecutionState>> GetStatesForExecution(int expectedStates) {
            return await GetStatesForExecution("", expectedStates);
        }

        private async Task<IList<IBenchmarkExecutionState>> GetStatesForExecution(string executionGuid, int expectedStates) {
            IList<IBenchmarkExecutionState> states = null;
            await Wait.Until(() => (states = WakekApplication.GetObservableCollectionSnapshot(s => s.BenchmarkExecutionGuid == executionGuid || executionGuid == "", () => WakekApplication.BenchmarkExecutionStates)).Count == expectedStates, TimeSpan.FromSeconds(1));
            Assert.AreEqual(expectedStates, states.Count);
            return states;
        }

        private async Task<IList<IDisplayedBenchmarkExecutionState>> GetDisplayedStatesForExecution(int expectedStates) {
            IList<IDisplayedBenchmarkExecutionState> states = null;
            await Wait.Until(() => (states = WakekApplication.GetObservableCollectionSnapshot(s => true, () => WakekApplication.DisplayedBenchmarkExecutionStates)).Count == expectedStates, TimeSpan.FromSeconds(1));
            Assert.AreEqual(expectedStates, states.Count);
            return states;
        }

        [TestMethod]
        public async Task CanExecuteCommandForNativeCsBenchmark() {
            var benchmarkDefinition = WakekApplication.BenchmarkDefinitions.FirstOrDefault(b => b.BenchmarkExecutionType == BenchmarkExecutionType.CsNative && !string.IsNullOrEmpty(b.Url));
            Assert.IsNotNull(benchmarkDefinition);
            WakekApplication.SelectBenchmarkDefinition(benchmarkDefinition);
            await ExecuteCommand.Execute(ApplicationCommandExecutionContext);
            VerifyExecutionStates();
        }

        [TestMethod]
        public async Task CanExecuteCommandForNativeCsBenchmarkWithFailingHttpCall() {
            var benchmarkDefinition = WakekApplication.BenchmarkDefinitions.FirstOrDefault(b => b.BenchmarkExecutionType == BenchmarkExecutionType.CsNative && !string.IsNullOrEmpty(b.Url));
            Assert.IsNotNull(benchmarkDefinition);
            WakekApplication.SelectBenchmarkDefinition(benchmarkDefinition);
            HttpClientGetStringSucceeds = false;
            await ExecuteCommand.Execute(ApplicationCommandExecutionContext);
            VerifyExecutionStates();
        }

        [TestMethod]
        public async Task CanExecuteCommandForJavaScriptBenchmark() {
            var benchmarkDefinition = WakekApplication.BenchmarkDefinitions.FirstOrDefault(b => b.BenchmarkExecutionType == BenchmarkExecutionType.JavaScript && !string.IsNullOrEmpty(b.Url));
            Assert.IsNotNull(benchmarkDefinition);
            WakekApplication.SelectBenchmarkDefinition(benchmarkDefinition);
            await ExecuteCommand.Execute(ApplicationCommandExecutionContext);
            VerifyExecutionStates();
        }

        protected void VerifyExecutionStates() {
            Assert.IsTrue(WakekApplication.BenchmarkExecutionStates.Any());
            Assert.IsTrue(HttpClientGetStringSucceeds
                ? WakekApplication.BenchmarkExecutionStates.All(b => b.Finished && b.Successes != 0 && b.Failures == 0)
                : WakekApplication.BenchmarkExecutionStates.All(b => b.Finished && b.Successes == 0 && b.Failures != 0));
        }
    }
}
