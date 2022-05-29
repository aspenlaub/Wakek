using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;

public class HttpClientFactory : IHttpClientFactory {
    public IHttpClient Create() {
        return new HttpClientProxy();
    }
}