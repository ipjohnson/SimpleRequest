namespace SimpleRequest.Runtime.Invoke;

public interface IRequestHandlerProvider {
    IRequestHandler? GetRequestHandler(IRequestContext context);
}
