using System.Net;
using DependencyModules.xUnit.Attributes;
using NSubstitute;
using SimpleRequest.Client.Filters;
using SimpleRequest.Client.Http;
using SimpleRequest.Client.Impl;
using Xunit;

namespace SimpleRequest.Client.Tests.Http;

public class HttpTransportChannelTests {
    [ModuleTest]
    public async Task BuildTransportChannelTest(
        IServiceProvider serviceProvider,
        [InjectValues("Channel")]HttpTransportChannel channel,
        [Mock] IHttpClientSendService sendService) {
        var operationInfo = new OperationInfo(
            "/info",
            "GET",
            typeof(HttpTransportChannelTests),
            null,
            null,
            null,
            ArraySegment<Attribute>.Empty,
            [],
            typeof(string)
        );

        var invokeDelegate = channel.GetInvokeDelegate<string>(operationInfo);
        
        var httpResponse = new HttpResponseMessage();

        httpResponse.StatusCode = HttpStatusCode.OK;
        
        var memoryStream = new MemoryStream();
        
        memoryStream.Write("\"Hello World\""u8);
        memoryStream.Position = 0;
        
        httpResponse.Content = new StreamContent(memoryStream);
        
        sendService.SendAsync(
            Arg.Any<ITransportFilterContext<HttpRequestMessage, HttpResponseMessage>>()).Returns(
            Task.FromResult(httpResponse));

        var result = await invokeDelegate(serviceProvider, "/path", new OperationParameters([]));
        
        Assert.Equal("Hello World", result);
    }
}