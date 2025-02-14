using System.Net;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Invoke.Impl;

[SingletonService]
public class NotFoundHandler : INotFoundHandler{
    private readonly IRequestContextSerializer _serializer;

    public NotFoundHandler(IRequestContextSerializer serializer) {
        _serializer = serializer;
    }

    public Task Handle(IRequestContext context) {
        if (context.ResponseData.ResponseStarted) {
            return Task.CompletedTask;
        }
        
        context.ResponseData.Status = (int)HttpStatusCode.NotFound;
        context.ResponseData.ResponseValue = new {
            message = "Not Found",
        };

        return _serializer.SerializeToResponse(context);
    }
}