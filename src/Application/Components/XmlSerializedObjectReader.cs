using System;
using System.Linq;
using System.Xml;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components {
    public class XmlSerializedObjectReader : IXmlSerializedObjectReader {
        private readonly IXmlDeserializer vXmlDeserializer;

        public XmlSerializedObjectReader(IXmlDeserializer xmlDeserializer) {
            vXmlDeserializer = xmlDeserializer;
        }

        public void IdentifyType(string s, out bool success, out Type t) {
            success = false;
            t = typeof(object);
            if (!s.StartsWith("<?xml")) { return; }

            try {
                var document = new XmlDocument();
                document.LoadXml(s);

                var rootElementName = document.DocumentElement?.Name;
                t = typeof(BenchmarkDefinition).Assembly.ExportedTypes.FirstOrDefault(x => x.Name == rootElementName);
                success = t != null;

                // ReSharper disable once EmptyGeneralCatchClause
            } catch {
            }
        }

        public T Read<T>(string s) {
            return vXmlDeserializer.Deserialize<T>(s);
        }
    }
}
