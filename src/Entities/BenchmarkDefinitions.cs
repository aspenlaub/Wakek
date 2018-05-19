using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Entities {
    [XmlRoot("BenchmarkDefinitions")]
    public class BenchmarkDefinitions : ObservableCollection<BenchmarkDefinition>, ISecretResult<BenchmarkDefinitions> {
        public BenchmarkDefinitions Clone() {
            var clone = new BenchmarkDefinitions();
            foreach (var benchmarkDefinition in this) {
                clone.Add(benchmarkDefinition);
            }

            return clone;
        }
    }
}
