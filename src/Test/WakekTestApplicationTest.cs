using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class WakekTestApplicationTest {
        protected WakekTestApplication Sut;

        [TestInitialize]
        public async Task Initialize() {
            Sut = new WakekTestApplication(null);
            await Sut.SetBenchmarkDefinitionsAsync();
        }

        [TestMethod]
        public async Task ThereIsAWakekTestApplication() {
            Assert.IsFalse(Sut.IsExecuting());
            Assert.AreEqual(0, Sut.Log.LogEntries.Count);
            Assert.AreEqual(4, Sut.BenchmarkDefinitions.Count);
            Assert.AreEqual(WakekTestApplication.TestBenchmarkGuid, Sut.BenchmarkDefinitions[0].Guid);
            Assert.AreEqual(WakekTestApplication.TestBenchmarkGuid, Sut.SelectedBenchmarkDefinition.Guid);
            Assert.AreEqual(WakekTestApplication.TestParallelBenchmarkGuid, Sut.BenchmarkDefinitions[1].Guid);
            Assert.AreEqual(1, await Sut.NavigateToStringReturnContentAsNumberAsync("url"));
        }

        [TestMethod]
        public async Task WakekTestApplicationHandlesLogAndCommandDisabledFeedback() {
            var feedback = new FeedbackToApplication { Type = FeedbackType.LogWarning, Message = "Warning" };
            var handled = await Sut.HandleFeedbackToApplicationReturnSuccessAsync(feedback);
            Assert.IsTrue(handled);
            feedback = new FeedbackToApplication { Type = FeedbackType.LogError, Message = "Error" };
            handled = await Sut.HandleFeedbackToApplicationReturnSuccessAsync(feedback);
            Assert.IsTrue(handled);
            feedback = new FeedbackToApplication { Type = FeedbackType.CommandIsDisabled, Message = "Disabled", CommandType = typeof(ExecuteCommand) };
            handled = await Sut.HandleFeedbackToApplicationReturnSuccessAsync(feedback);
            Assert.IsTrue(handled);
        }
    }
}
