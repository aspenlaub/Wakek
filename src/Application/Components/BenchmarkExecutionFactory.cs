using System;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components {
    public class BenchmarkExecutionFactory : IBenchmarkExecutionFactory {
        protected IWakekComponentProvider WakekComponentProvider;

        public BenchmarkExecutionFactory(IWakekComponentProvider wakekComponentProvider) {
            WakekComponentProvider = wakekComponentProvider;
        }

        public IBenchmarkExecution CreateBenchmarkExecution(IBenchmarkDefinition benchmarkDefinition, int threadNumber) {
            return new BenchmarkExecution {
                SequenceNumber = WakekComponentProvider.SequenceNumberGenerator.NewSequenceNumber(nameof(BenchmarkExecution)),
                Guid = Guid.NewGuid().ToString(),
                BenchmarkDefinitionGuid = benchmarkDefinition.Guid,
                ThreadNumber = threadNumber
            };
        }

        public IBenchmarkExecutionState CreateBenchmarkExecutionState(IBenchmarkExecution benchmarkExecution) {
            return new BenchmarkExecutionState {
                SequenceNumber = WakekComponentProvider.SequenceNumberGenerator.NewSequenceNumber(nameof(BenchmarkExecutionState)),
                BenchmarkExecutionGuid = benchmarkExecution.Guid,
                ExecutingForHowManySeconds = 0,
                Failures = 0, Successes = 0,
                RemoteExecutingForHowManySeconds = 0, RemoteRequiringForHowManySeconds = 0,
                Finished = false,
            };

        }
    }
}
