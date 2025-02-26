namespace SimpleRequest.Runtime.Invoke;

public interface IRequestHandlerProvider {
    int Order => 0;
    
    IEnumerable<IRequestHandler> GetHandlers();
    
    IRequestHandler? GetRequestHandler(IRequestContext context);
}
