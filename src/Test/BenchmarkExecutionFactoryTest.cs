using System;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test;

[TestClass]
public class BenchmarkExecutionFactoryTest {
    private readonly IContainer _Container = new ContainerBuilder().UseWakek().Build();

    [TestMethod]
    public void NeedBenchmarkDefinitionGuidForCreationOfExecution() {
        var benchmarkDefinitionMock = new Mock<IBenchmarkDefinition>();
        benchmarkDefinitionMock.SetupGet(b => b.Guid).Returns("");
        var sut = new BenchmarkExecutionFactory(_Container.Resolve<ISequenceNumberGenerator>());
        Assert.ThrowsExactly<NullReferenceException>(() => {
            sut.CreateBenchmarkExecution(benchmarkDefinitionMock.Object);
        }, "", "");
    }

    [TestMethod]
    public void NeedBenchmarkExecutionGuidForCreationOfState() {
        var benchmarkExecutionMock = new Mock<IBenchmarkExecution>();
        benchmarkExecutionMock.SetupGet(b => b.Guid).Returns("");
        var sut = new BenchmarkExecutionFactory(_Container.Resolve<ISequenceNumberGenerator>());
        Assert.ThrowsExactly<NullReferenceException>(() => {
            sut.CreateBenchmarkExecutionState(benchmarkExecutionMock.Object, 1);
        }, "", "");
    }
}