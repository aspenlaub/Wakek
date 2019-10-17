using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;
using Moq;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test {
    public class FakeHttpClientFactory : IHttpClientFactory {
        private readonly Func<bool> vHttpClientGetStringSucceeds;

        public FakeHttpClientFactory(Func<bool> httpClientGetStringSucceeds) {
            vHttpClientGetStringSucceeds = httpClientGetStringSucceeds;
        }

        private Task<string> HttpClientGetStringResult() {
            if (vHttpClientGetStringSucceeds()) { return Task.FromResult("Hello World"); }

            throw new Exception("Http call failed");
        }

        public IHttpClient Create() {
            var httpClientMock = new Mock<IHttpClient>();
            httpClientMock.SetupGet(h => h.BaseAddress);
            httpClientMock.Setup(h => h.GetStringAsync(It.IsAny<string>())).Returns<string>(s => HttpClientGetStringResult());
            return httpClientMock.Object;
        }
    }
}
