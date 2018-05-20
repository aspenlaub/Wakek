using System;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components {
    public interface IXmlSerializedObjectReader {
        void IdentifyType(string s, out bool success, out Type t);
        T Read<T>(string s);
    }
}
