namespace SimpleRequest.Runtime.Invoke;

public interface IRequestHandler {
    IRequestHandlerInfo RequestHandlerInfo { get; }
    
    Task Invoke(IRequestContext context);
}