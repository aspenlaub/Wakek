using System.Collections.Concurrent;
using System.Collections.Generic;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components {
    public class SequenceNumberGenerator : ISequenceNumberGenerator {
        protected IDictionary<string, int> NextSequenceNumbers;
        protected object LockObject = new object();

        public SequenceNumberGenerator() {
            NextSequenceNumbers = new ConcurrentDictionary<string, int>();
        }

        public int NewSequenceNumber(string sequenceName) {
            lock(LockObject) {
                if (!NextSequenceNumbers.ContainsKey(sequenceName)) {
                    NextSequenceNumbers.Add(sequenceName, 1);
                }

                return NextSequenceNumbers[sequenceName] ++;
            }
        }
    }
}
