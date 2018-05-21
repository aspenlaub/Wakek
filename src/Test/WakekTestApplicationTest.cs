using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
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
            Assert.AreEqual(1, Sut.BenchmarkDefinitions.Count);
            Assert.AreEqual(WakekTestApplication.TestBenchmarkGuid, Sut.BenchmarkDefinitions[0].Guid);
            Assert.AreEqual(WakekTestApplication.TestBenchmarkGuid, Sut.SelectedBenchmarkDefinition.Guid);
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
        public async Task ExecuteCommendCreatesExecutionAndState() {
            Assert.IsNotNull(Sut);
            Assert.AreEqual(0, Sut.BenchmarkExecutions.Count);
            await Sut.ApplicationCommandController.Execute(typeof(ExecuteCommand));
            await Sut.ApplicationCommandController.AwaitAllAsynchronousTasks();
            Assert.AreEqual(1, Sut.BenchmarkExecutions.Count);
            var executionGuid = Sut.BenchmarkExecutions[0].Guid;
            var states = Sut.BenchmarkExecutionStates.Where(s => s.BenchmarkExecutionGuid == executionGuid).ToList();
            Assert.AreEqual(1, states.Count);
            bool handled;
            var feedback = new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = Sut.WakekComponentProvider.PeghComponentProvider.XmlSerializer.Serialize(states[0] as BenchmarkExecutionState) };
            Sut.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
            Assert.AreEqual(1, Sut.BenchmarkExecutionStates.Count);
            var execution = Sut.WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecution(Sut.BenchmarkDefinitions[0], 1);
            var state = Sut.WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecutionState(execution) as BenchmarkExecutionState;
            feedback = new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = Sut.WakekComponentProvider.PeghComponentProvider.XmlSerializer.Serialize(state) };
            Sut.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
            Assert.AreEqual(2, Sut.BenchmarkExecutionStates.Count);
        }
    }
}
