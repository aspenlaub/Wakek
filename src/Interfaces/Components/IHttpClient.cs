using System;
using System.Threading.Tasks;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

public interface IHttpClient : IDisposable {
    Uri BaseAddress { get; set; }

    Task<string> GetStringAsync(string url);
}