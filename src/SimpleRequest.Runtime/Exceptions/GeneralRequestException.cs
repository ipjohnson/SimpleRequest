using System.Net;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Exceptions;

public interface IRequestException {
    int StatusCode { get; }

    object? ResponseData(IRequestContext context) {
        return null;
    }
}

public class GeneralRequestException(int statusCode, string message) : Exception(message), IRequestException {
    public int StatusCode { get; } = statusCode;
}