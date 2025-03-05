using System.Net;

namespace SimpleRequest.Runtime.Exceptions;

public class GeneralRequestException(int statusCode, string message) : Exception(message) {
    public int StatusCode { get; } = statusCode;
}