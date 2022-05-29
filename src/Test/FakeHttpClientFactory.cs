using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test;

public class FakeHttpClientFactory : IHttpClientFactory {
    private readonly Func<bool> HttpClientGetStringSucceeds;

    public FakeHttpClientFactory(Func<bool> httpClientGetStringSucceeds) {
        HttpClientGetStringSucceeds = httpClientGetStringSucceeds;
    }

    private Task<string> HttpClientGetStringResult() {
        if (HttpClientGetStringSucceeds()) { return Task.FromResult("Hello World"); }

        throw new Exception("Http call failed");
    }

    public IHttpClient Create() {
        var httpClientMock = new Mock<IHttpClient>();
        httpClientMock.SetupGet(h => h.BaseAddress);
        httpClientMock.Setup(h => h.GetStringAsync(It.IsAny<string>())).Returns<string>(_ => HttpClientGetStringResult());
        return httpClientMock.Object;
    }
}