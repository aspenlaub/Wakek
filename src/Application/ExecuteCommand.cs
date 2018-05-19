using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application {
    public class ExecuteCommand : IApplicationCommand {
        protected IExecuteCommandContext ContextOwner;
        protected XmlSerializer XmlSerializer;

        public bool MakeLogEntries => true;
        public string Name => Properties.Resources.ExecuteCommandName;

        public ExecuteCommand(IExecuteCommandContext contextOwner) {
            ContextOwner = contextOwner;
            XmlSerializer = new XmlSerializer();
        }

        public bool CanExecute() {
            return !ContextOwner.IsExecuting();
        }

        public Task Execute(IApplicationCommandExecutionContext context) {
            return Task.Run(() => {
                var benchmarkExecution = new BenchmarkExecution {
                    SequenceNumber = ContextOwner.NewSequenceNumber(),
                    Guid = Guid.NewGuid().ToString(),
                    BenchmarkDefinitionGuid = ContextOwner.SelectedBenchmarkDefinition.Guid,
                    ThreadNumber = 1
                };
                context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecution) } );
            });
        }
    }
}
