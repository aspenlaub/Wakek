﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Autofac;
using mshtml;
#pragma warning disable 4014

namespace Aspenlaub.Net.GitHub.CSharp.Wakek {
    /// <summary>
    /// Interaction logic for WakekWindow.xaml
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public partial class WakekWindow {
        protected ApplicationCommandController Controller;
        protected WakekApplication WakekApplication;
        protected ViewSources WakekViewSources;
        protected SynchronizationContext UiSynchronizationContext;
        protected string HtmlOutputContentDivInnerHtml;

        public WakekWindow() {
            Controller = new ApplicationCommandController(ApplicationFeedbackHandler);
            UiSynchronizationContext = SynchronizationContext.Current;
            var container = new ContainerBuilder().UseWakek().Build();
            WakekApplication = new WakekApplication(Controller, Controller, UiSynchronizationContext, NavigateToStringReturnContentAsNumber,
                container.Resolve<ISecretRepository>(), container.Resolve<IXmlSerializedObjectReader>(), container.Resolve<IBenchmarkExecutionFactory>(),
                container.Resolve<IXmlSerializer>(), container.Resolve<ITelemetryDataReader>(), container.Resolve<IHttpClientFactory>());

            InitializeComponent();
        }

        public void ApplicationFeedbackHandler(IFeedbackToApplication feedback) {
            WakekApplication.ApplicationFeedbackHandler(feedback, out var handled);
            if (handled) { return; }

            switch (feedback.Type) {
                case FeedbackType.CommandExecutionCompleted: {
                        CommandExecutionCompletedHandler(feedback);
                    }
                    break;
                case FeedbackType.CommandsEnabledOrDisabled: {
                        CommandsEnabledOrDisabledHandler();
                    }
                    break;
                default: {
                        throw new NotImplementedException();
                    }
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void CommandExecutionCompletedHandler(IFeedbackToApplication feedback) {
            if (!Controller.IsMainThread()) { return; }

            Cursor = Cursors.Arrow;
        }

        public void CommandsEnabledOrDisabledHandler() {
            Execute.IsEnabled = Controller.Enabled(typeof(ExecuteCommand));
        }

        private void WakekWindow_OnLoaded(object sender, RoutedEventArgs e) {
            WakekViewSources = new ViewSources(this);
            SetViewSource(WakekViewSources.BenchmarkDefinitionViewSource, WakekApplication.BenchmarkDefinitions, "Description", ListSortDirection.Ascending);
            SetViewSource(WakekViewSources.BenchmarkExecutionStateViewSource, WakekApplication.DisplayedBenchmarkExecutionStates, "SequenceNumber", ListSortDirection.Ascending);
            SetViewSource(WakekViewSources.LogViewSource, WakekApplication.Log.LogEntries, "SequenceNumber", ListSortDirection.Ascending);
            CommandsEnabledOrDisabledHandler();
        }
        private void SetViewSource<T>(CollectionViewSource source, ObservableCollection<T> collection, string sortProperty, ListSortDirection sortDirection) {
            source.Source = collection;
            source.SortDescriptions.Clear();
            source.SortDescriptions.Add(new SortDescription(sortProperty, sortDirection));
        }

        private async void Execute_OnClick(object sender, RoutedEventArgs e) {
            Cursor = Cursors.Wait;
            await Controller.Execute(typeof(ExecuteCommand));
        }

        private void SelectedBenchmarkDefinition_OnDropDownClosed(object sender, EventArgs e) {
            var item = SelectedBenchmarkDefinition.SelectedItem as IBenchmarkDefinition;
            WakekApplication.SelectBenchmarkDefinition(item);
        }

        private int NavigateToStringReturnContentAsNumber(string html) {
            if (UiSynchronizationContext == SynchronizationContext.Current) { return 0; }

            UiSynchronizationContext.Post(state => RefreshWebBrowserContents(), null);
            Thread.Sleep(TimeSpan.FromMilliseconds(300));
            var initialContents = HtmlOutputContentDivInnerHtml ?? "..";

            UiSynchronizationContext.Send(state => HtmlOutput.NavigateToString(html), null);
            Thread.Sleep(TimeSpan.FromMilliseconds(1000));
            string oldContents;
            var newContents = "";
            do {
                UiSynchronizationContext.Post(state => RefreshWebBrowserContents(), null);
                Thread.Sleep(TimeSpan.FromMilliseconds(300));

                oldContents = newContents;
                newContents = HtmlOutputContentDivInnerHtml ?? "..";
                if (newContents != initialContents) {
                    initialContents = "..";
                }
            } while (oldContents != newContents || newContents.Contains("..") || newContents == initialContents);

            int.TryParse(newContents, out var result);
            return result;
        }

        private void RefreshWebBrowserContents() {
            var document = (HTMLDocumentClass)HtmlOutput.Document;
            if (document?.body == null) { return; }

            var element = document.getElementById("content");
            HtmlOutputContentDivInnerHtml = element?.innerHTML;
        }
    }
}
