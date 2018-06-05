using System;
using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components {
    public class WakekComponentProvider : IWakekComponentProvider {
        private Dictionary<Type, object> DefaultComponents { get; }
        public IComponentProvider PeghComponentProvider { get; }

        public WakekComponentProvider(IComponentProvider peghComponentProvider) {
            DefaultComponents = new Dictionary<Type, object>();
            PeghComponentProvider = peghComponentProvider;
        }

        private T DefaultComponent<T, T2>() where T : class where T2 : T, new() {
            if (!DefaultComponents.ContainsKey(typeof(T))) {
                DefaultComponents[typeof(T)] = new T2();
            }
            return (T)DefaultComponents[typeof(T)];
        }

        // ReSharper disable once UnusedMember.Local
        private T DefaultComponent<T, T2>(Func<T2> constructor) where T : class where T2 : T {
            if (!DefaultComponents.ContainsKey(typeof(T))) {
                DefaultComponents[typeof(T)] = constructor();
            }
            return (T)DefaultComponents[typeof(T)];
        }

        public IBenchmarkExecutionFactory BenchmarkExecutionFactory { get { return DefaultComponent<IBenchmarkExecutionFactory, BenchmarkExecutionFactory>(() => new BenchmarkExecutionFactory(this)); } }
        public IHttpClient HttpClient { get { return new HttpClientProxy(); } }
        public ISequenceNumberGenerator SequenceNumberGenerator { get { return DefaultComponent<ISequenceNumberGenerator, SequenceNumberGenerator>(); } }
        public IXmlSerializedObjectReader XmlSerializedObjectReader { get { return DefaultComponent<IXmlSerializedObjectReader, XmlSerializedObjectReader>(() => new XmlSerializedObjectReader(PeghComponentProvider)); } }
    }
}
