using System.Windows;
using System.Windows.Data;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek {
    public class ViewSources {
        public CollectionViewSource BenchmarkDefinitionViewSource;
        public CollectionViewSource BenchmarkExecutionStateViewSource;
        public CollectionViewSource LogViewSource;

        public ViewSources(FrameworkElement window) {
            BenchmarkDefinitionViewSource = window.FindResource("BenchmarkDefinitionViewSource") as CollectionViewSource;
            BenchmarkExecutionStateViewSource = window.FindResource("BenchmarkExecutionStateViewSource") as CollectionViewSource;
            LogViewSource = window.FindResource("LogViewSource") as CollectionViewSource;
        }
    }
}
