using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Filters;

public interface ITransportFilterContext<TRequest, TResponse> : IOperationFilterContext {
    
    TRequest? TransportRequest { get; set; }
    
    TResponse? TransportResponse { get; set;  }
}


public interface ITransportFilter<TRequest, TResponse> {
    int Order => 0;
    
    bool SupportOperation(string channel, IOperationInfo operation);

    Task Invoke(ITransportFilterContext<TRequest, TResponse> context);
}