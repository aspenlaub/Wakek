using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

public interface ITelemetryDataReader {
    Task<IList<ITelemetryData>> ReadAsync(IBenchmarkDefinition benchmarkDefinition);
}