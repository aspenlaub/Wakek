﻿using System.Xml.Serialization;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Entities {
    public class BenchmarkExecution : IBenchmarkExecution {
        [XmlAttribute("sequencenumber")]
        public int SequenceNumber { get; }

        [XmlAttribute("guid")]
        public string Guid { get; set; }

        [XmlAttribute("definition")]
        public string BenchmarkDefinitionGuid { get; set; }

        [XmlAttribute("thread")]
        public int ThreadNumber { get; set; }
    }
}
