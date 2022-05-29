using System.Windows;
using System.Windows.Data;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek;

public class ViewSources {
    public CollectionViewSource BenchmarkDefinitionViewSource;
    public CollectionViewSource BenchmarkExecutionStateViewSource;

    public ViewSources(FrameworkElement window) {
        BenchmarkDefinitionViewSource = window.FindResource("BenchmarkDefinitionViewSource") as CollectionViewSource;
        BenchmarkExecutionStateViewSource = window.FindResource("BenchmarkExecutionStateViewSource") as CollectionViewSource;
    }
}