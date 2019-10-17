using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Entities {
    public class TelemetryData : ITelemetryData {
        public int RequiringForHowManyMiliSeconds { get; set; }
        public int ExecutingForHowManyMiliSeconds { get; set; }
    }
}
