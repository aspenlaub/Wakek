using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Skladasu.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test;

[TestClass]
public class BenchmarkDefinitionTest {
    [TestMethod]
    public async Task HaveBenchmarkDefinitionsSecret() {
        var secret = new SecretBenchmarkDefinitions();
        IContainer container = new ContainerBuilder().UsePegh("Wakek").Build();
        var errorsAndInfos = new ErrorsAndInfos();
        await container.Resolve<ISecretRepository>().GetAsync(secret, errorsAndInfos);
        Assert.IsFalse(errorsAndInfos.AnyErrors(), string.Join("\r\n", errorsAndInfos.Errors));
    }

    [TestMethod]
    public void ThereIsADefaultBenchmarkDefinitionListThatCanBeCloned() {
        var secret = new SecretBenchmarkDefinitions();
        BenchmarkDefinitions definitions = secret.DefaultValue;
        Assert.IsTrue(definitions.Count != 0);

        BenchmarkDefinitions clone = definitions.Clone();
        Assert.AreEqual(definitions.Count, clone.Count);
        Assert.AreEqual(definitions[0].Guid, clone[0].Guid);
        Assert.AreEqual(definitions[0].BenchmarkExecutionType, clone[0].BenchmarkExecutionType);
        Assert.AreEqual(definitions[0].Url, clone[0].Url);
        Assert.AreEqual(definitions[0].Description, clone[0].Description);
        Assert.AreEqual(definitions[0].ExecutionTimeInSeconds, clone[0].ExecutionTimeInSeconds);
        Assert.AreEqual(definitions[0].NumberOfCallsInParallel, clone[0].NumberOfCallsInParallel);
    }
}