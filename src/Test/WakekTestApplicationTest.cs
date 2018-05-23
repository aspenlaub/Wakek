using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class WakekTestApplicationTest {
        protected WakekTestApplication Sut;

        [TestInitialize]
        public void Initialize() {
            Sut = new WakekTestApplication();
        }

        [TestMethod]
        public void ThereIsAWakekTestApplication() {
            Assert.IsFalse(Sut.IsExecuting());
            Assert.AreEqual(0, Sut.Log.LogEntries.Count);
            Assert.AreEqual(2, Sut.BenchmarkDefinitions.Count);
            Assert.AreEqual(WakekTestApplication.TestBenchmarkGuid, Sut.BenchmarkDefinitions[0].Guid);
            Assert.AreEqual(WakekTestApplication.TestBenchmarkGuid, Sut.SelectedBenchmarkDefinition.Guid);
            Assert.AreEqual(WakekTestApplication.TestParallelBenchmarkGuid, Sut.BenchmarkDefinitions[1].Guid);
            Assert.IsNotNull(Sut.WakekComponentProvider);
        }

        [TestMethod]
        public void WakekTestApplicationHandlesLogAndCommandDisabledFeedback() {
            bool handled;
            var feedback = new FeedbackToApplication { Type = FeedbackType.LogWarning, Message = "Warning" };
            Sut.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
            feedback = new FeedbackToApplication { Type = FeedbackType.LogError, Message = "Error" };
            Sut.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
            feedback = new FeedbackToApplication { Type = FeedbackType.CommandIsDisabled, Message = "Disabled", CommandType = typeof(ExecuteCommand) };
            Sut.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
        }

        [TestMethod]
        public async Task ExecuteCommandCreatesExecutionAndState() {
            Assert.IsNotNull(Sut);
            Assert.AreEqual(0, Sut.BenchmarkExecutions.Count);
            await Sut.ApplicationCommandController.Execute(typeof(ExecuteCommand));
            Assert.AreEqual(1, Sut.BenchmarkExecutions.Count);
            var executionGuid = Sut.BenchmarkExecutions[0].Guid;
            var states = await GetStatesForExecution(executionGuid, 1);
            var state = states[0] as BenchmarkExecutionState;
            Assert.IsNotNull(state);
            var displayedStates = await GetDisplayedStatesForExecution(1);
            Assert.AreEqual(Sut.SelectedBenchmarkDefinition.Description, displayedStates[0].BenchmarkDescription);

            bool handled;
            var feedback = new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = Sut.WakekComponentProvider.PeghComponentProvider.XmlSerializer.Serialize(state) };
            Sut.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
            await GetStatesForExecution(executionGuid, 1);

            var execution = Sut.WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecution(Sut.BenchmarkDefinitions[0]);
            state = Sut.WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecutionState(execution, 1) as BenchmarkExecutionState;
            feedback = new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = Sut.WakekComponentProvider.PeghComponentProvider.XmlSerializer.Serialize(state) };
            Sut.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
            await GetStatesForExecution(2);
        }

        [TestMethod]
        public async Task ExecuteCommandCreatesExecutionAndStateForMultiThreadBenchmarks() {
            Assert.IsNotNull(Sut);
            Assert.AreEqual(0, Sut.BenchmarkExecutions.Count);
            Sut.SelectBenchmarkDefinition(Sut.BenchmarkDefinitions[1]);
            await Sut.ApplicationCommandController.Execute(typeof(ExecuteCommand));
            Assert.AreEqual(1, Sut.BenchmarkExecutions.Count);
            var executionGuid = Sut.BenchmarkExecutions[0].Guid;

            var states = await GetStatesForExecution(executionGuid, 2);
            Assert.IsTrue(states.Any(s => s.ThreadNumber == 1));
            Assert.IsTrue(states.Any(s => s.ThreadNumber == 2));

            var displayedStates = await GetDisplayedStatesForExecution(1);
            Assert.AreEqual(Sut.SelectedBenchmarkDefinition.Description, displayedStates[0].BenchmarkDescription);
        }

        private async Task<IList<IBenchmarkExecutionState>> GetStatesForExecution(int expectedStates) {
            return await GetStatesForExecution("", expectedStates);
        }

        private async Task<IList<IBenchmarkExecutionState>> GetStatesForExecution(string executionGuid, int expectedStates) {
            IList<IBenchmarkExecutionState> states = null;
            await Wait.Until(() => (states = Sut.GetObservableCollectionSnapshot(s => s.BenchmarkExecutionGuid == executionGuid || executionGuid == "", () => Sut.BenchmarkExecutionStates)).Count == expectedStates, TimeSpan.FromSeconds(1));
            Assert.AreEqual(expectedStates, states.Count);
            return states;
        }

        private async Task<IList<IDisplayedBenchmarkExecutionState>> GetDisplayedStatesForExecution(int expectedStates) {
            IList<IDisplayedBenchmarkExecutionState> states = null;
            await Wait.Until(() => (states = Sut.GetObservableCollectionSnapshot(s => true, () => Sut.DisplayedBenchmarkExecutionStates)).Count == expectedStates, TimeSpan.FromSeconds(1));
            Assert.AreEqual(expectedStates, states.Count);
            return states;
        }
    }
}
