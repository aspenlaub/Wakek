using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class WakekComponentProviderTest {
        [TestMethod]
        public void CanProvideComponents() {
            var sut = new WakekComponentProvider(new ComponentProvider());
            Assert.IsNotNull(sut.BenchmarkExecutionFactory);
            Assert.IsNotNull(sut.HttpClient);
            Assert.IsNotNull(sut.PeghComponentProvider);
            Assert.IsNotNull(sut.SequenceNumberGenerator);
            Assert.IsNotNull(sut.TelemetryDataParser);
            Assert.IsNotNull(sut.TelemetryDataReader);
        }
    }
}
