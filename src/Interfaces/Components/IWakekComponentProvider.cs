﻿using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components {
    public interface IWakekComponentProvider {
        IComponentProvider PeghComponentProvider { get; }
        ISequenceNumberGenerator SequenceNumberGenerator { get; }
        IXmlSerializedObjectReader XmlSerializedObjectReader { get; }
    }
}
