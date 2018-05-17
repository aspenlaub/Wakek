using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Entities.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Basic.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek {
    /// <summary>
    /// Interaction logic for WakekWindow.xaml
    /// </summary>
    public partial class WakekWindow {
        protected IComponentProvider ComponentProvider;
        protected ApplicationCommandController Controller;
        protected SynchronizationContext UiSynchronizationContext;
        protected WakekApplication WakekApplication;
        protected ViewSources WakekViewSources;

        public WakekWindow() {
            UiSynchronizationContext = SynchronizationContext.Current;
            ComponentProvider = new ComponentProvider();
            Controller = new ApplicationCommandController(ApplicationFeedbackHandler);
            WakekApplication = new WakekApplication(ComponentProvider, Controller, Controller, UiSynchronizationContext);

            InitializeComponent();
        }

        public void ApplicationFeedbackHandler(IFeedbackToApplication feedback) {
            switch (feedback.Type) {
                case FeedbackType.CommandExecutionCompleted: {
                    CommandExecutionCompletedHandler(feedback);
                }
                break;
                case FeedbackType.CommandsEnabledOrDisabled: {
                    CommandsEnabledOrDisabledHandler();
                }
                break;
                case FeedbackType.LogInformation: {
                    WakekApplication.Log.Add(new LogEntry { Message = feedback.Message, CreatedAt = feedback.CreatedAt, SequenceNumber = feedback.SequenceNumber });
                }
                break;
                case FeedbackType.LogWarning: {
                    WakekApplication.Log.Add(new LogEntry { Class = LogEntryClass.Warning, Message = feedback.Message, CreatedAt = feedback.CreatedAt, SequenceNumber = feedback.SequenceNumber });
                }
                break;
                case FeedbackType.LogError: {
                    WakekApplication.Log.Add(new LogEntry { Class = LogEntryClass.Error, Message = feedback.Message, CreatedAt = feedback.CreatedAt, SequenceNumber = feedback.SequenceNumber });
                }
                break;
                case FeedbackType.CommandIsDisabled: {
                    WakekApplication.Log.Add(new LogEntry { Class = LogEntryClass.Error, Message = "Attempt to run disabled command " + feedback.CommandType, CreatedAt = feedback.CreatedAt, SequenceNumber = feedback.SequenceNumber });
                }
                break;
                default: {
                    throw new NotImplementedException();
                }
            }
        }

        private void CommandExecutionCompletedHandler(IFeedbackToApplication feedback) {
            if (!Controller.IsMainThread()) { return; }

            throw new NotImplementedException();
        }

        public void CommandsEnabledOrDisabledHandler() {
            Execute.IsEnabled = Controller.Enabled(typeof(ExecuteCommand));
        }

        private void WakekWindow_OnLoaded(object sender, RoutedEventArgs e) {
            WakekViewSources = new ViewSources(this);
            SetViewSource(WakekViewSources.BenchmarkDefinitionViewSource, WakekApplication.ObservableBenchmarkDefinitions, "Description", ListSortDirection.Ascending);
            SetViewSource(WakekViewSources.BenchmarkExecutionStateViewSource, WakekApplication.Log.LogEntries, "SequenceNumber", ListSortDirection.Ascending);
            SetViewSource(WakekViewSources.LogViewSource, WakekApplication.Log.LogEntries, "SequenceNumber", ListSortDirection.Ascending);
            CommandsEnabledOrDisabledHandler();
        }
        private void SetViewSource<T>(CollectionViewSource source, ObservableCollection<T> collection, string sortProperty, ListSortDirection sortDirection) {
            source.Source = collection;
            source.SortDescriptions.Clear();
            source.SortDescriptions.Add(new SortDescription(sortProperty, sortDirection));
        }

        private void Execute_OnClick(object sender, RoutedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}
