using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;

public class BenchmarkExecutionState : IBenchmarkExecutionState {
    [XmlAttribute("sequencenumber")]
    public int SequenceNumber { get; set; }

    [XmlAttribute("guid")]
    public string BenchmarkExecutionGuid { get; set; }

    [XmlAttribute("thread")]
    public int ThreadNumber { get; set; }

    public string Guid => BenchmarkExecutionGuid + '_' + ThreadNumber;

    [XmlAttribute("secondsexecuting")]
    public int ExecutingForHowManySeconds { get; set; }

    [XmlAttribute("secondsexecutingremote")]
    public int RemoteExecutingForHowManySeconds { get; set; }

    [XmlAttribute("secondsrequiringremote")]
    public int RemoteRequiringForHowManySeconds { get; set; }

    [XmlAttribute("successes")]
    public int Successes { get; set; }

    [XmlAttribute("failures")]
    public int Failures { get; set; }

    [XmlAttribute("finished")]
    public bool Finished { get; set; }
}