using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Autofac;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;

public static class WakekContainerBuilder {
    public static ContainerBuilder UseWakek(this ContainerBuilder builder) {
        builder.UsePegh("Wakek");
        builder.RegisterType<BenchmarkExecutionFactory>().As<IBenchmarkExecutionFactory>();
        builder.RegisterType<HttpClientFactory>().As<IHttpClientFactory>();
        builder.RegisterType<SequenceNumberGenerator>().As<ISequenceNumberGenerator>();
        builder.RegisterType<TelemetryDataParser>().As<ITelemetryDataParser>();
        builder.RegisterType<TelemetryDataReader>().As<ITelemetryDataReader>();
        builder.RegisterType<XmlSerializedObjectReader>().As<IXmlSerializedObjectReader>();
        return builder;
    }
}