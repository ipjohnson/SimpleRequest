using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Impl;

public interface ITransportChannel {
    Task Initialize();
    
    string ChannelName { get; }
    
    InvokeDelegate<T?> GetInvokeDelegate<T>(IOperationInfo operation);
}