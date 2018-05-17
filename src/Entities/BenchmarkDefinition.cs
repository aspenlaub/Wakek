﻿using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Entities {
    public class BenchmarkDefinition : IBenchmarkDefinition {
        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("url")]
        public string Url { get; set; }

        [XmlElement("executiontype")]
        public BenchmarkExecutionType BenchmarkExecutionType { get; set; }

        [XmlElement("timeinseconds")]
        public int ExecutionTimeInSeconds { get; set; }

        [XmlElement("callsinparallel")]
        public int NumberOfCallsInParallel { get; set; }
    }
}
