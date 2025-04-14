using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Filters;

namespace SimpleRequest.Runtime.Invoke.Impl;

[SingletonService]
public class RequestHandlerFactory : IRequestHandlerFactory {
    private readonly IRequestFilterManagementService _requestFilterManagementService;

    public RequestHandlerFactory(IRequestFilterManagementService requestFilterManagementService) {
        _requestFilterManagementService = requestFilterManagementService;
    }

    public IRequestHandler GetHandler(IRequestHandlerInfo requestHandlerInfo, string handlerType) {
        return new RequestHandler(requestHandlerInfo, _requestFilterManagementService.GetFilters(requestHandlerInfo));
    }
}