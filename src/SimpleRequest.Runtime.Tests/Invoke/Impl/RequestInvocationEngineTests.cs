using DependencyModules.xUnit.Attributes;
using NSubstitute;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Invoke.Impl;
using SimpleRequest.Runtime.Tests.Models;
using Xunit;

namespace SimpleRequest.Runtime.Tests.Invoke.Impl;

public class RequestInvocationEngineTests {
    private class Handler : IRequestHandler {

        public bool InvokeCalled { get; set; } = false;

        public IRequestHandlerInfo RequestHandlerInfo {
            get;
        } = default!;

        public Task Invoke(IRequestContext context) {
            InvokeCalled = true;
            
            return Task.CompletedTask;
        }
    }
    
    [ModuleTest]
    public async Task InvokeWithOneProvider(
        RequestInvocationEngine engine,
        [Mock]IRequestHandlerProvider provider,
        IServiceProvider serviceProvider) {
        var handler = new Handler();
        provider.GetRequestHandler(Arg.Any<IRequestContext>()).Returns(handler);

        await engine.Invoke(TestRequestContext.GenerateTestContext(serviceProvider));
        
        Assert.True(handler.InvokeCalled);
    }
    
    [ModuleTest]
    public async Task InvokeNotFound(
        RequestInvocationEngine engine,
        IServiceProvider serviceProvider) {

        var context = TestRequestContext.GenerateTestContext(serviceProvider);
        await engine.Invoke(context);
        
        Assert.Equal(404, context.ResponseData.Status);
    }
}