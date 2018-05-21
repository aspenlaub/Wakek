using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application {
    public class ExecuteCommand : IApplicationCommand {
        protected IExecuteCommandContext ContextOwner;
        protected IWakekComponentProvider WakekComponentProvider;
        protected XmlSerializer XmlSerializer;

        public bool MakeLogEntries => true;
        public string Name => Properties.Resources.ExecuteCommandName;

        public ExecuteCommand(IExecuteCommandContext contextOwner) {
            ContextOwner = contextOwner;
            WakekComponentProvider = ContextOwner.WakekComponentProvider;
            XmlSerializer = new XmlSerializer();
        }

        public bool CanExecute() {
            return !ContextOwner.IsExecuting();
        }

        public Task Execute(IApplicationCommandExecutionContext context) {
            return Task.Run(() => {
                var benchmarkExecution = WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecution(ContextOwner.SelectedBenchmarkDefinition, 1) as BenchmarkExecution;
                var benchmarkExecutionState = WakekComponentProvider.BenchmarkExecutionFactory.CreateBenchmarkExecutionState(benchmarkExecution) as BenchmarkExecutionState;
                context.Report(new FeedbackToApplication {Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecution)});
                context.Report(new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = XmlSerializer.Serialize(benchmarkExecutionState) });
            });
        }
    }
}
