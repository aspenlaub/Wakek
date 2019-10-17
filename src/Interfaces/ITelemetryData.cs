namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces {
    public interface ITelemetryData {
        int RequiringForHowManyMiliSeconds { get; set; }
        int ExecutingForHowManyMiliSeconds { get; set; }
    }
}
