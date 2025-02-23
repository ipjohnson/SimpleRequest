namespace SimpleRequest.Runtime.Invoke;

public interface IRequestHandlerLocator {
    IRequestHandler? GetHandler(IRequestContext context);
}