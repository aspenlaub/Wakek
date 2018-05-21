using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces {
    public interface IBenchmarkExecution : IGuid {
        int SequenceNumber { get; }
        string BenchmarkDefinitionGuid { get; }
    }
}
