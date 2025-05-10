using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Impl;

public interface IInvokeStrategyLocator {
    
    InvokeDelegate<T?> GetInvokeDelegate<T>(string channelName, IOperationInfo operation);
}

public class InvokeStrategyLocator(IReadOnlyList<ITransportChannel> transportChannels) : IInvokeStrategyLocator {

    public InvokeDelegate<T?> GetInvokeDelegate<T>(string channelName, IOperationInfo operation) {
        for (var i = 0; i < transportChannels.Count; i++) {
            if (transportChannels[i].ChannelName == channelName) {
                return transportChannels[i].GetInvokeDelegate<T>(operation);
            }
        }
        
        throw new Exception($"Channel {channelName} not found");
    }
}