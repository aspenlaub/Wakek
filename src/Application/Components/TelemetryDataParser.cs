using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using CsvHelper;
using CsvHelper.Configuration;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components {
    public class TelemetryDataParser : ITelemetryDataParser {
        public bool TryParse(string s, out IList<ITelemetryData> result) {
            try {
                var configuration = new CsvConfiguration(CultureInfo.CurrentCulture) {
                    Delimiter = ";"
                };
                var csvReader = new CsvReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(s))), configuration);
                result = csvReader.GetRecords<TelemetryData>().Cast<ITelemetryData>().ToList();
                return true;
            } catch {
                result = new List<ITelemetryData>();
                return false;
            }
        }
    }
}
