using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;

public class TelemetryData : ITelemetryData {
    public int RequiringForHowManyMilliSeconds { get; set; }
    public int ExecutingForHowManyMilliSeconds { get; set; }
}