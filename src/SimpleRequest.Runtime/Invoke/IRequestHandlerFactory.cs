namespace SimpleRequest.Runtime.Invoke;

public interface IRequestHandlerFactory {
    IRequestHandler GetHandler(IRequestHandlerInfo requestHandlerInfo, string handlerType);
}