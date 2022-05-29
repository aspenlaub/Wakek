using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Helpers;
using Aspenlaub.Net.GitHub.CSharp.Pegh.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Application;
using Aspenlaub.Net.GitHub.CSharp.Vishizhukel.Interfaces.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Entities;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test;

[TestClass]
public class ExecuteCommandTest {
    protected WakekTestApplication WakekApplication;
    protected ExecuteCommand ExecuteCommand;
    protected IApplicationCommandExecutionContext ApplicationCommandExecutionContext;
    protected bool HttpClientGetStringSucceeds;

    private readonly IContainer Container;

    public ExecuteCommandTest() {
        Container = new ContainerBuilder().UseWakek().Build();
    }

    [TestInitialize]
    public async Task Initialize() {
        HttpClientGetStringSucceeds = true;
        var factory = new FakeHttpClientFactory(() => HttpClientGetStringSucceeds);
        WakekApplication = new WakekTestApplication(factory);
        await WakekApplication.SetBenchmarkDefinitionsAsync();
        ExecuteCommand = new ExecuteCommand(WakekApplication, Container.Resolve<IBenchmarkExecutionFactory>(), Container.Resolve<IXmlSerializer>(),
            Container.Resolve<ITelemetryDataReader>(), factory);
        var applicationCommandExecutionContextMock = new Mock<IApplicationCommandExecutionContext>();
        applicationCommandExecutionContextMock.Setup(a => a.ReportAsync(It.IsAny<IFeedbackToApplication>()))
            .Returns<IFeedbackToApplication>(async f => await WakekApplication.HandleFeedbackToApplicationAsync(f));
        ApplicationCommandExecutionContext = applicationCommandExecutionContextMock.Object;
    }

    [TestMethod]
    public async Task ExecuteCommandCreatesExecutionAndState() {
        Assert.IsNotNull(WakekApplication);
        Assert.AreEqual(0, WakekApplication.BenchmarkExecutions.Count);
        await WakekApplication.ApplicationCommandController.ExecuteAsync(typeof(ExecuteCommand));
        Assert.AreEqual(1, WakekApplication.BenchmarkExecutions.Count);
        var executionGuid = WakekApplication.BenchmarkExecutions[0].Guid;
        var states = GetStatesForExecution(executionGuid, 1);
        var state = states[0] as BenchmarkExecutionState;
        Assert.IsNotNull(state);
        var displayedStates = GetDisplayedStatesForExecution(1);
        var displayedState = displayedStates[0];
        Assert.AreEqual(WakekApplication.SelectedBenchmarkDefinition.Description, displayedState.BenchmarkDescription);
        Assert.AreEqual(state.ExecutingForHowManySeconds, displayedState.ExecutingForHowManySeconds);
        Assert.AreEqual(state.Failures, displayedState.Failures);
        Assert.AreEqual(state.Successes, displayedState.Successes);
        Assert.AreEqual(state.RemoteExecutingForHowManySeconds, displayedState.RemoteExecutingForHowManySeconds);
        Assert.AreEqual(state.RemoteRequiringForHowManySeconds, displayedState.RemoteRequiringForHowManySeconds);
        Assert.AreEqual(state.Finished, displayedState.Finished);

        var feedback = new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = Container.Resolve<IXmlSerializer>().Serialize(state) };
        var handled = await WakekApplication.HandleFeedbackToApplicationReturnSuccessAsync(feedback);
        Assert.IsTrue(handled);
        GetStatesForExecution(executionGuid, 1);

        var execution = Container.Resolve<IBenchmarkExecutionFactory>().CreateBenchmarkExecution(WakekApplication.BenchmarkDefinitions[0]);
        state = Container.Resolve<IBenchmarkExecutionFactory>().CreateBenchmarkExecutionState(execution, 1) as BenchmarkExecutionState;
        feedback = new FeedbackToApplication { Type = FeedbackType.ImportantMessage, Message = Container.Resolve<IXmlSerializer>().Serialize(state) };
        handled = await WakekApplication.HandleFeedbackToApplicationReturnSuccessAsync(feedback);
        Assert.IsTrue(handled);
        GetStatesForExecution(2);
    }

    [TestMethod]
    public async Task ExecuteCommandCreatesExecutionAndStateForMultiThreadBenchmarks() {
        Assert.IsNotNull(WakekApplication);
        Assert.AreEqual(0, WakekApplication.BenchmarkExecutions.Count);
        WakekApplication.SelectBenchmarkDefinition(WakekApplication.BenchmarkDefinitions[1]);
        await WakekApplication.ApplicationCommandController.ExecuteAsync(typeof(ExecuteCommand));
        Assert.AreEqual(1, WakekApplication.BenchmarkExecutions.Count);
        var executionGuid = WakekApplication.BenchmarkExecutions[0].Guid;

        var states = GetStatesForExecution(executionGuid, 2);
        Assert.IsTrue(states.Any(s => s.ThreadNumber == 1));
        Assert.IsTrue(states.Any(s => s.ThreadNumber == 2));

        var displayedStates = GetDisplayedStatesForExecution(1);
        Assert.AreEqual(WakekApplication.SelectedBenchmarkDefinition.Description, displayedStates[0].BenchmarkDescription);
    }

    // ReSharper disable once UnusedMethodReturnValue.Local
    private IList<IBenchmarkExecutionState> GetStatesForExecution(int expectedStates) {
        return GetStatesForExecution("", expectedStates);
    }

    private IList<IBenchmarkExecutionState> GetStatesForExecution(string executionGuid, int expectedStates) {
        IList<IBenchmarkExecutionState> states = null;
        Wait.Until(() => (states = WakekApplication.GetObservableCollectionSnapshot(s => s.BenchmarkExecutionGuid == executionGuid || executionGuid == "", () => WakekApplication.BenchmarkExecutionStates)).Count == expectedStates, TimeSpan.FromSeconds(1));
        Assert.AreEqual(expectedStates, states.Count);
        return states;
    }

    private IList<IDisplayedBenchmarkExecutionState> GetDisplayedStatesForExecution(int expectedStates) {
        IList<IDisplayedBenchmarkExecutionState> states = null;
        Wait.Until(() => (states = WakekApplication.GetObservableCollectionSnapshot(_ => true, () => WakekApplication.DisplayedBenchmarkExecutionStates)).Count == expectedStates, TimeSpan.FromSeconds(1));
        Assert.AreEqual(expectedStates, states.Count);
        return states;
    }

    [TestMethod]
    public async Task CanExecuteCommandForNativeCsBenchmark() {
        var benchmarkDefinition = WakekApplication.BenchmarkDefinitions.FirstOrDefault(b => b.BenchmarkExecutionType == BenchmarkExecutionType.CsNative && !string.IsNullOrEmpty(b.Url));
        Assert.IsNotNull(benchmarkDefinition);
        WakekApplication.SelectBenchmarkDefinition(benchmarkDefinition);
        await ExecuteCommand.ExecuteAsync(ApplicationCommandExecutionContext);
        VerifyExecutionStates();
    }

    [TestMethod]
    public async Task CanExecuteCommandForNativeCsBenchmarkWithFailingHttpCall() {
        var benchmarkDefinition = WakekApplication.BenchmarkDefinitions.FirstOrDefault(b => b.BenchmarkExecutionType == BenchmarkExecutionType.CsNative && !string.IsNullOrEmpty(b.Url));
        Assert.IsNotNull(benchmarkDefinition);
        WakekApplication.SelectBenchmarkDefinition(benchmarkDefinition);
        HttpClientGetStringSucceeds = false;
        await ExecuteCommand.ExecuteAsync(ApplicationCommandExecutionContext);
        VerifyExecutionStates();
    }

    [TestMethod]
    public async Task CanExecuteCommandForJavaScriptBenchmark() {
        var benchmarkDefinition = WakekApplication.BenchmarkDefinitions.FirstOrDefault(b => b.BenchmarkExecutionType == BenchmarkExecutionType.JavaScript && !string.IsNullOrEmpty(b.Url));
        Assert.IsNotNull(benchmarkDefinition);
        WakekApplication.SelectBenchmarkDefinition(benchmarkDefinition);
        await ExecuteCommand.ExecuteAsync(ApplicationCommandExecutionContext);
        VerifyExecutionStates();
    }

    protected void VerifyExecutionStates() {
        Assert.IsTrue(WakekApplication.BenchmarkExecutionStates.Any());
        Assert.IsTrue(HttpClientGetStringSucceeds
            ? WakekApplication.BenchmarkExecutionStates.All(b => b.Finished && b.Successes != 0 && b.Failures == 0)
            : WakekApplication.BenchmarkExecutionStates.All(b => b.Finished && b.Successes == 0 && b.Failures != 0));
    }
}