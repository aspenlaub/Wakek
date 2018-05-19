using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application {
    public class ExecuteCommand : IApplicationCommand {
        protected IExecuteCommandContext ContextOwner;

        public bool MakeLogEntries => true;
        public string Name => Properties.Resources.ExecuteCommandName;

        public ExecuteCommand(IExecuteCommandContext contextOwner) {
            ContextOwner = contextOwner;
        }

        public bool CanExecute() {
            return !ContextOwner.IsExecuting();
        }

        public Task Execute(IApplicationCommandExecutionContext context) {
            return Task.Run(() => {
                throw new NotImplementedException();
            });
        }
    }
}
