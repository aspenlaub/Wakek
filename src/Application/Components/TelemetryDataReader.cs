using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components {
    public class TelemetryDataReader : ITelemetryDataReader {
        protected readonly IWakekComponentProvider ComponentProvider;

        public TelemetryDataReader(IWakekComponentProvider componentProvider) {
            ComponentProvider = componentProvider;
        }

        public async Task<IList<ITelemetryData>> ReadAsync(IBenchmarkDefinition benchmarkDefinition) {
            if (string.IsNullOrEmpty(benchmarkDefinition?.Url) || string.IsNullOrEmpty(benchmarkDefinition.TelemetryUrl)) { return new List<ITelemetryData>(); }

            var url = benchmarkDefinition.Url;
            url = url.Substring(url.IndexOf(@"//", StringComparison.Ordinal) + 2);
            if (string.IsNullOrEmpty(url)) { return new List<ITelemetryData>(); }

            url = url.Substring(url.IndexOf(@"/", StringComparison.Ordinal) + 1);
            if (string.IsNullOrEmpty(url)) { return new List<ITelemetryData>(); }

            url = benchmarkDefinition.TelemetryUrl + "?url=" + HttpUtility.UrlEncode(url);

            var client = new HttpClient();
            var data = await client.GetStringAsync(url);
            if (string.IsNullOrEmpty(data)) { return new List<ITelemetryData>(); }

            IList<ITelemetryData> result;
            return !ComponentProvider.TelemetryDataParser.TryParse(data, out result) ? new List<ITelemetryData>() : result;
        }
    }
}
