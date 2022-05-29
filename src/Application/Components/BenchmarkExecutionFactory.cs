using System;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;

public class BenchmarkExecutionFactory : IBenchmarkExecutionFactory {
    private readonly ISequenceNumberGenerator SequenceNumberGenerator;

    public BenchmarkExecutionFactory(ISequenceNumberGenerator sequenceNumberGenerator) {
        SequenceNumberGenerator = sequenceNumberGenerator;
    }

    public IBenchmarkExecution CreateBenchmarkExecution(IBenchmarkDefinition benchmarkDefinition) {
        if (string.IsNullOrEmpty(benchmarkDefinition.Guid)) {
            throw new NullReferenceException("benchmarkDefinition.Guid");
        }

        return new BenchmarkExecution {
            SequenceNumber = SequenceNumberGenerator.NewSequenceNumber(nameof(BenchmarkExecution)),
            Guid = Guid.NewGuid().ToString(),
            BenchmarkDefinitionGuid = benchmarkDefinition.Guid,
        };
    }

    public IBenchmarkExecutionState CreateBenchmarkExecutionState(IBenchmarkExecution benchmarkExecution, int threadNumber) {
        if (string.IsNullOrEmpty(benchmarkExecution.Guid)) {
            throw new NullReferenceException("benchmarkExecution.Guid");
        }

        return new BenchmarkExecutionState {
            SequenceNumber = SequenceNumberGenerator.NewSequenceNumber(nameof(BenchmarkExecutionState)),
            BenchmarkExecutionGuid = benchmarkExecution.Guid, ThreadNumber = threadNumber,
            ExecutingForHowManySeconds = 0,
            Failures = 0, Successes = 0,
            RemoteExecutingForHowManySeconds = 0, RemoteRequiringForHowManySeconds = 0,
            Finished = false,
        };

    }
}