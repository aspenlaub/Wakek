namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

public interface ITelemetryData {
    int RequiringForHowManyMilliSeconds { get; set; }
    int ExecutingForHowManyMilliSeconds { get; set; }
}