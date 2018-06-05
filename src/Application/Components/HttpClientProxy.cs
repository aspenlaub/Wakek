using System;
using System.Net.Http;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Interfaces.Components;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components {
    public class HttpClientProxy : IHttpClient {
        protected HttpClient HttpClient;

        public HttpClientProxy() {
            HttpClient = new HttpClient();
        }

        public async Task<string> GetStringAsync(string url) {
            return await HttpClient.GetStringAsync(url);
        }

        public Uri BaseAddress {
            get { return HttpClient.BaseAddress; }
            set { HttpClient.BaseAddress = value; }
        }
    }
}
