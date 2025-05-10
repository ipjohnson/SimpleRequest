using Microsoft.Extensions.Primitives;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Model;

public class OperationRequest {
    public OperationRequest(
        IOperationInfo operation,
        string path,
        IOperationParameters parameters,
        IDictionary<string, StringValues> headers) {
        Operation = operation;
        Path = path;
        Parameters = parameters;
        Headers = headers;
    }

    public IOperationInfo Operation { get; set; }

    public string Path { get; set; }

    public IOperationParameters Parameters { get; set; }

    public IDictionary<string, StringValues> Headers { get; set; }
}

public class OperationResponse {
    public OperationResponse(
        IOperationInfo operation,
        int? statusCode,
        IDictionary<string, StringValues> headers) {
        Operation = operation;
        StatusCode = statusCode;
        Headers = headers;
    }

    public IOperationInfo Operation { get; set; }

    public int? StatusCode { get; set; }

    public IDictionary<string, StringValues> Headers { get; set; }

    
    public object? Result { get; set; }
    
    public Exception? Exception { get; set; }
}