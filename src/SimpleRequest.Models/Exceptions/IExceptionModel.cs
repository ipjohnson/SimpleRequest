namespace SimpleRequest.Models.Exceptions;

public interface IExceptionModel {
    int StatusCode { get; }
    
    object? GetData();
}