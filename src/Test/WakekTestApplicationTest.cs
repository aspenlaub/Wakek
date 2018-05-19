using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
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
        public async Task ExecuteCommendCreatesExecution() {
            Assert.IsNotNull(Sut);
            Assert.AreEqual(0, Sut.BenchmarkExecutions.Count);
            await Sut.ApplicationCommandController.Execute(typeof(ExecuteCommand));
            Assert.AreEqual(1, Sut.BenchmarkExecutions.Count);
        }
    }
}
