using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class WakekTestApplicationTest {
        [TestMethod]
        public void ThereIsAWakekTestApplication() {
            var sut = new WakekTestApplication();
            Assert.IsNotNull(sut);
        }
    }
}
