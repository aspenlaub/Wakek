using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class SequenceNumberGeneratorTest {
        protected const string SequenceName = "ThisIsNotASequence";
        protected const string OtherSequenceName = "IAmNotASequenceEither";

        [TestMethod]
        public async Task CanGenerateSequenceNumbers() {
            var sut = new SequenceNumberGenerator();
            Assert.AreEqual(1, sut.NewSequenceNumber(SequenceName));
            Assert.AreEqual(2, sut.NewSequenceNumber(SequenceName));
            Assert.AreEqual(1, sut.NewSequenceNumber(OtherSequenceName));
            var sequenceNumbers = (await Task.WhenAll(Enumerable.Range(0, 1000).Select(_ => Task.Run(() => sut.NewSequenceNumber(SequenceName))))).ToList();
            Assert.AreEqual(sequenceNumbers.Distinct().Count(), sequenceNumbers.Count);
            Assert.IsTrue(Enumerable.Range(0, 1000).All(i => sequenceNumbers.Contains(i + 3)));
        }
    }
}
