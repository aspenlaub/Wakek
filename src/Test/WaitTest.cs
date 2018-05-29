using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class WaitTest {
        [TestMethod]
        public async Task CanWait() {
            var begin = DateTime.Now;
            await Wait.Until(() => false, TimeSpan.FromSeconds(2));
            var millsecondsElapsed = DateTime.Now.Subtract(begin).TotalMilliseconds;
            Assert.IsTrue(millsecondsElapsed >= 2000 && millsecondsElapsed <= 2100);
        }
    }
}
