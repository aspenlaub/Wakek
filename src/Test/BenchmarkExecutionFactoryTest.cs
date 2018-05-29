using System;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class BenchmarkExecutionFactoryTest {
        [TestMethod, ExpectedException(typeof(NullReferenceException))]
        public void NeedBenchmarkDefinitionGuidForCreationOfExecution() {
            var componentProviderMock = new Mock<IWakekComponentProvider>();
            var benchmarkDefinitionMock = new Mock<IBenchmarkDefinition>();
            benchmarkDefinitionMock.SetupGet(b => b.Guid).Returns("");
            var sut = new BenchmarkExecutionFactory(componentProviderMock.Object);
            sut.CreateBenchmarkExecution(benchmarkDefinitionMock.Object);
        }

        [TestMethod, ExpectedException(typeof(NullReferenceException))]
        public void NeedBenchmarkExecutionGuidForCreationOfState() {
            var componentProviderMock = new Mock<IWakekComponentProvider>();
            var benchmarkExecutionMock = new Mock<IBenchmarkExecution>();
            benchmarkExecutionMock.SetupGet(b => b.Guid).Returns("");
            var sut = new BenchmarkExecutionFactory(componentProviderMock.Object);
            sut.CreateBenchmarkExecutionState(benchmarkExecutionMock.Object, 1);
        }
    }
}
