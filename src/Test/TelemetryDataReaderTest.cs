using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class TelemetryDataReaderTest {
        [TestMethod]
        public async Task CanReadTelemetryData() {
            var componentProviderMock = new Mock<IWakekComponentProvider>();
            componentProviderMock.SetupGet(c => c.TelemetryDataParser).Returns(new TelemetryDataParser());
            var sut = new TelemetryDataReader(componentProviderMock.Object);
            var benchmarkDefinition = new BenchmarkDefinition() {
                Url = "https://www.viperfisch.de/wakek/helloworld.php",
                TelemetryUrl = "https://www.viperfisch.de/wakek/gettelemetrydata.php"
            };
            var client = new HttpClient();
            await client.GetStringAsync(benchmarkDefinition.Url);
            var resultOne = await sut.ReadAsync(benchmarkDefinition);
            Assert.IsTrue(resultOne.Count != 0);
            await client.GetStringAsync(benchmarkDefinition.Url);
            var resultTwo = await sut.ReadAsync(benchmarkDefinition);
            Assert.IsTrue(resultTwo.Count > resultOne.Count);
            Assert.IsTrue(resultOne.All(r => resultTwo.Any(r2 => r.RequiringForHowManyMilliSeconds == r2.RequiringForHowManyMilliSeconds && r.ExecutingForHowManyMilliSeconds == r2.ExecutingForHowManyMilliSeconds)));
        }
    }
}
