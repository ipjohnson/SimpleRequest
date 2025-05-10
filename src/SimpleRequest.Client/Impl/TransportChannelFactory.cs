using System.Collections.Concurrent;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;
using SimpleRequest.Client.Exceptions;

namespace SimpleRequest.Client.Impl;

public interface ITransportChannelFactory {
    Task<ITransportChannel> Channel(string channelName);
}

[SingletonService]
public class TransportChannelFactory(IServiceProvider serviceProvider) : ITransportChannelFactory {
    private readonly ConcurrentDictionary<string, ITransportChannel> _channels = new ();
    
    public async Task<ITransportChannel> Channel(string channelName) {
        return _channels.GetOrAdd(channelName, c => {
            var channel = serviceProvider.GetKeyedService<ITransportChannel>(c);

            if (channel == null) {
                throw new MissingChannelException(c);
            }
            return channel;
        });
    }
}