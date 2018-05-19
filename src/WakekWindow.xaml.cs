using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Components;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;

#pragma warning disable 4014

namespace Aspenlaub.Net.GitHub.CSharp.Wakek {
    /// <summary>
    /// Interaction logic for WakekWindow.xaml
    /// </summary>
    public partial class WakekWindow {
        protected ApplicationCommandController Controller;
        protected WakekApplication WakekApplication;
        protected ViewSources WakekViewSources;

        public WakekWindow() {
            Controller = new ApplicationCommandController(ApplicationFeedbackHandler);
            WakekApplication = new WakekApplication(new ComponentProvider(), Controller, Controller, SynchronizationContext.Current);

            InitializeComponent();
        }

        public void ApplicationFeedbackHandler(IFeedbackToApplication feedback) {
            bool handled;
            WakekApplication.ApplicationFeedbackHandler(feedback, out handled);
            if (handled) { return; }

            switch (feedback.Type) {
                case FeedbackType.CommandExecutionCompleted: {
                    CommandExecutionCompletedHandler(feedback);
                } break;
                case FeedbackType.CommandsEnabledOrDisabled: {
                    CommandsEnabledOrDisabledHandler();
                } break;
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
            SetViewSource(WakekViewSources.BenchmarkDefinitionViewSource, WakekApplication.BenchmarkDefinitions, "Description", ListSortDirection.Ascending);
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
            Controller.Execute(typeof(ExecuteCommand));
        }
    }
}
