namespace SimpleRequest.Runtime.Invoke;

public interface IRequestInvocationEngine {
    Task Invoke(IRequestContext context);
}