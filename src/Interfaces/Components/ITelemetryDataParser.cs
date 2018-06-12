using System.Collections.Generic;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components {
    public interface ITelemetryDataParser {
        bool TryParse(string s, out IList<ITelemetryData> result);
    }
}
