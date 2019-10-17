using System;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class BenchmarkExecutionFactoryTest {
        private readonly IContainer vContainer;

        public BenchmarkExecutionFactoryTest() {
            vContainer = new ContainerBuilder().UseWakek().Build();
        }

        [TestMethod, ExpectedException(typeof(NullReferenceException))]
        public void NeedBenchmarkDefinitionGuidForCreationOfExecution() {
            var benchmarkDefinitionMock = new Mock<IBenchmarkDefinition>();
            benchmarkDefinitionMock.SetupGet(b => b.Guid).Returns("");
            var sut = new BenchmarkExecutionFactory(vContainer.Resolve<ISequenceNumberGenerator>());
            sut.CreateBenchmarkExecution(benchmarkDefinitionMock.Object);
        }

        [TestMethod, ExpectedException(typeof(NullReferenceException))]
        public void NeedBenchmarkExecutionGuidForCreationOfState() {
            var benchmarkExecutionMock = new Mock<IBenchmarkExecution>();
            benchmarkExecutionMock.SetupGet(b => b.Guid).Returns("");
            var sut = new BenchmarkExecutionFactory(vContainer.Resolve<ISequenceNumberGenerator>());
            sut.CreateBenchmarkExecutionState(benchmarkExecutionMock.Object, 1);
        }
    }
}
