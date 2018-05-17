using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    [TestClass]
    public class BenchmarkDefinitionTest {
        [TestMethod]
        public void HaveBenchmarkDefinitionsSecret() {
            var secret = new SecretBenchmarkDefinitions();
            var componentProvider = new ComponentProvider();
            var errorsAndInfos = new ErrorsAndInfos();
            componentProvider.SecretRepository.Get(secret, errorsAndInfos);
            Assert.IsFalse(errorsAndInfos.AnyErrors(), string.Join("\r\n", errorsAndInfos.Errors));
        }
    }
}
