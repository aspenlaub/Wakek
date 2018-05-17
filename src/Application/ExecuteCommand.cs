using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application {
    public class ExecuteCommand : IApplicationCommand {
        protected IExecute ContextOwner;

        public bool MakeLogEntries => true;
        public string Name => Properties.Resources.ExecuteCommandName;

        public ExecuteCommand(IExecute contextOwner) {
            ContextOwner = contextOwner;
        }

        public bool CanExecute() {
            return !ContextOwner.IsExecuting();
        }

        public Task Execute(IApplicationCommandExecutionContext context) {
            return Task.Run(() => {
                ContextOwner.Execute();
            });
        }
    }
}
