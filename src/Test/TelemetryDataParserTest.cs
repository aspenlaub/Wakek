using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class TelemetryDataParserTest {
        [TestMethod]
        public void CanParseTelemetryData() {
            var sut = new TelemetryDataParser();
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Aspenlaub.Net.GitHub.CSharp.Wakek.Test.telemetrydata.csv")) {
                Assert.IsNotNull(stream);
                using (var reader = new StreamReader(stream)) {
                    IList<ITelemetryData> result;
                    Assert.IsTrue(sut.TryParse(reader.ReadToEnd(), out result));
                    Assert.AreEqual(39, result.Count);
                    Assert.AreEqual(27, result[0].RequiringForHowManyMilliSeconds);
                    Assert.AreEqual(23, result[result.Count - 1].ExecutingForHowManyMilliSeconds);
                }
            }
        }

        [TestMethod]
        public void NoTelemetryDataIfInvalidCsv() {
            var sut = new TelemetryDataParser();
            IList<ITelemetryData> result;
            Assert.IsFalse(sut.TryParse("This is not comma-separated", out result));
            Assert.AreEqual(0, result.Count);
        }
    }
}
