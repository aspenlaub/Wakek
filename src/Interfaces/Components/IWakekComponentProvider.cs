using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components {
    public interface IWakekComponentProvider {
        IBenchmarkExecutionFactory BenchmarkExecutionFactory { get; }
        IComponentProvider PeghComponentProvider { get; }
        IHttpClient HttpClient { get; }
        ISequenceNumberGenerator SequenceNumberGenerator { get; }
        ITelemetryDataParser TelemetryDataParser { get; }
        ITelemetryDataReader TelemetryDataReader { get; }
        IXmlSerializedObjectReader XmlSerializedObjectReader { get; }
    }
}
