using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class WakekTestApplicationTest {
        protected WakekTestApplication Sut;

        [TestInitialize]
        public void Initialize() {
            Sut = new WakekTestApplication(null);
        }

        [TestMethod]
        public void ThereIsAWakekTestApplication() {
            Assert.IsFalse(Sut.IsExecuting());
            Assert.AreEqual(0, Sut.Log.LogEntries.Count);
            Assert.AreEqual(4, Sut.BenchmarkDefinitions.Count);
            Assert.AreEqual(WakekTestApplication.TestBenchmarkGuid, Sut.BenchmarkDefinitions[0].Guid);
            Assert.AreEqual(WakekTestApplication.TestBenchmarkGuid, Sut.SelectedBenchmarkDefinition.Guid);
            Assert.AreEqual(WakekTestApplication.TestParallelBenchmarkGuid, Sut.BenchmarkDefinitions[1].Guid);
            Assert.AreEqual(1, Sut.NavigateToStringReturnContentAsNumber("url"));
        }

        [TestMethod]
        public void WakekTestApplicationHandlesLogAndCommandDisabledFeedback() {
            var feedback = new FeedbackToApplication { Type = FeedbackType.LogWarning, Message = "Warning" };
            Sut.ApplicationFeedbackHandler(feedback, out var handled);
            Assert.IsTrue(handled);
            feedback = new FeedbackToApplication { Type = FeedbackType.LogError, Message = "Error" };
            Sut.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
            feedback = new FeedbackToApplication { Type = FeedbackType.CommandIsDisabled, Message = "Disabled", CommandType = typeof(ExecuteCommand) };
            Sut.ApplicationFeedbackHandler(feedback, out handled);
            Assert.IsTrue(handled);
        }
    }
}
