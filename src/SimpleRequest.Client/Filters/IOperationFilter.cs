using SimpleRequest.Client.Model;
using SimpleRequest.Client.Serialization;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Filters;

public interface IOperationFilterContext {
    string ChannelName { get; }
    
    IContentSerializer ContentSerializer { get; }
    
    OperationRequest OperationRequest { get; }
    
    OperationResponse OperationResponse { get; }
    
    Task Next();
}

public interface IOperationFilter {
    int Order => 0;
    
    bool SupportOperation(string channelName, IOperationInfo operation);
    
    Task Invoke(IOperationFilterContext context);
}