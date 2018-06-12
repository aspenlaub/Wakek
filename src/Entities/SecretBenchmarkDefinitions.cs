using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Entities {
    public class SecretBenchmarkDefinitions : ISecret<BenchmarkDefinitions> {
        private BenchmarkDefinitions vDefaultValue;

        public BenchmarkDefinitions DefaultValue
            => vDefaultValue
            ?? (vDefaultValue = new BenchmarkDefinitions {
                new BenchmarkDefinition {
                    BenchmarkExecutionType = BenchmarkExecutionType.CsNative,
                    Description = "Hello World from viperfisch.de",
                    Url = "https://www.viperfisch.de/wakek/helloworld.php",
                    ExecutionTimeInSeconds = 5,
                    NumberOfCallsInParallel = 1,
                    TelemetryUrl ="https://www.viperfisch.de/wakek/gettelemetry.php"
                }
            });

        public string Guid => "E050F157-6DB3-479A-BBA5-1DEC4F2786B6";
    }
}
