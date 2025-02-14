namespace SimpleRequest.Runtime.Invoke;

public interface INotFoundHandler {
    Task Handle(IRequestContext context);
}