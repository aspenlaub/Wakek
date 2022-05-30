using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
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

namespace Aspenlaub.Net.GitHub.CSharp.Wakek;

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
        var container = new ContainerBuilder().UseWakek().Build();
        var simpleLogger = container.Resolve<ISimpleLogger>();
        Controller = new ApplicationCommandController(simpleLogger, HandleFeedbackToApplicationAsync);
        UiSynchronizationContext = SynchronizationContext.Current;
        WakekApplication = new WakekApplication(Controller, Controller, UiSynchronizationContext, NavigateToStringReturnContentAsNumber,
            container.Resolve<ISecretRepository>(), container.Resolve<IXmlSerializedObjectReader>(), container.Resolve<IBenchmarkExecutionFactory>(),
            container.Resolve<IXmlSerializer>(), container.Resolve<ITelemetryDataReader>(), container.Resolve<IHttpClientFactory>(), simpleLogger);

        InitializeComponent();
    }

    public async Task HandleFeedbackToApplicationAsync(IFeedbackToApplication feedback) {
        var handled = await WakekApplication.HandleFeedbackToApplicationReturnSuccessAsync(feedback);
        if (handled) { return; }

        switch (feedback.Type) {
            case FeedbackType.CommandExecutionCompleted: {
                CommandExecutionCompletedHandler(feedback);
            } break;
            case FeedbackType.CommandsEnabledOrDisabled: {
                await CommandsEnabledOrDisabledHandlerAsync();
            } break;
            default: {
                throw new NotImplementedException();
            }
        }

        await Task.CompletedTask;
    }

    // ReSharper disable once UnusedParameter.Local
    private void CommandExecutionCompletedHandler(IFeedbackToApplication feedback) {
        if (!Controller.IsMainThread()) { return; }

        Cursor = Cursors.Arrow;
    }

    public async Task CommandsEnabledOrDisabledHandlerAsync() {
        Execute.IsEnabled = await Controller.EnabledAsync(typeof(ExecuteCommand));
    }

    private async void OnWakekWindowLoadedAsync(object sender, RoutedEventArgs e) {
        await WakekApplication.SetBenchmarkDefinitionsAsync();

        WakekViewSources = new ViewSources(this);
        SetViewSource(WakekViewSources.BenchmarkDefinitionViewSource, WakekApplication.BenchmarkDefinitions, "Description", ListSortDirection.Ascending);
        SetViewSource(WakekViewSources.BenchmarkExecutionStateViewSource, WakekApplication.DisplayedBenchmarkExecutionStates, "SequenceNumber", ListSortDirection.Ascending);
        await CommandsEnabledOrDisabledHandlerAsync();
    }

    private void SetViewSource<T>(CollectionViewSource source, ObservableCollection<T> collection, string sortProperty, ListSortDirection sortDirection) {
        source.Source = collection;
        source.SortDescriptions.Clear();
        source.SortDescriptions.Add(new SortDescription(sortProperty, sortDirection));
    }

    private async void OnExecuteClick(object sender, RoutedEventArgs e) {
        Cursor = Cursors.Wait;
        await WebView.EnsureCoreWebView2Async();
        await Controller.ExecuteAsync(typeof(ExecuteCommand));
    }

    private void OnSelectedBenchmarkDefinitionDropDownClosed(object sender, EventArgs e) {
        var item = SelectedBenchmarkDefinition.SelectedItem as IBenchmarkDefinition;
        WakekApplication.SelectBenchmarkDefinition(item);
    }

    private async Task<int> NavigateToStringReturnContentAsNumber(string html) {
        if (UiSynchronizationContext == SynchronizationContext.Current) { return 0; }

        await RefreshWebBrowserContentsAsync();
        await Task.Delay(TimeSpan.FromMilliseconds(300));
        var initialContents = HtmlOutputContentDivInnerHtml ?? "..";

        UiSynchronizationContext.Send(_ => WebView.NavigateToString(html), null);
        await Task.Delay(TimeSpan.FromMilliseconds(1000));
        string oldContents;
        var newContents = "";
        do {
            await RefreshWebBrowserContentsAsync();
            await Task.Delay(TimeSpan.FromMilliseconds(300));

            oldContents = newContents;
            newContents = HtmlOutputContentDivInnerHtml ?? "..";
            if (newContents != initialContents) {
                initialContents = "..";
            }
        } while (oldContents != newContents || newContents.Contains("..") || newContents == initialContents);

        int.TryParse(newContents, out var result);
        return result;
    }

    private async Task RefreshWebBrowserContentsAsync() {
        HtmlOutputContentDivInnerHtml = await WebView.ExecuteScriptAsync("document.getElementById('content').innerHTML");
    }
}