using System.Collections.Generic;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Entities {
    [XmlRoot("BenchmarkDefinitions")]
    public class BenchmarkDefinitions : List<BenchmarkDefinition>, ISecretResult<BenchmarkDefinitions> {
        public BenchmarkDefinitions Clone() {
            var clone = new BenchmarkDefinitions();
            clone.AddRange(this);
            return clone;
        }
    }
}
