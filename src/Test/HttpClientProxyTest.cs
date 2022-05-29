using System;
using System.Threading.Tasks;
using Aspenlaub.Net.GitHub.CSharp.Wakek.Application.Components;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Aspenlaub.Net.GitHub.CSharp.Wakek.Test;

[TestClass]
public class HttpClientProxyTest {
    [TestMethod]
    public async Task CanUseHttpClientProxy() {
        var uri = new Uri(@"https://www.viperfisch.de/wakek/");
        var sut = new HttpClientProxy { BaseAddress = uri };
        Assert.AreEqual(uri, sut.BaseAddress);
        var result = await sut.GetStringAsync("helloworld.php");
        Assert.AreEqual("Hello World", result);
    }
}