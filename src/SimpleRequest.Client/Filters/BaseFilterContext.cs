using SimpleRequest.Client.Impl;
using SimpleRequest.Client.Model;
using SimpleRequest.Client.Serialization;

namespace SimpleRequest.Client.Filters;

public class FilterContext<TChannelRequest, TChannelResponse>(
    string channelName,
    IPathBuilder pathBuilder,
    IContentSerializer contentSerializer,
    IServiceProvider serviceProvider,
    OperationRequest operationRequest,
    OperationResponse operationResponse,
    IOperationFilter[] operationFilters,
    ITransportFilter<TChannelRequest, TChannelResponse>[] transportFilters)
    : ITransportFilterContext<TChannelRequest, TChannelResponse> {
    private int _filterIndex;

    public string ChannelName {
        get;
    } = channelName;

    public IPathBuilder PathBuilder {
        get;
    } = pathBuilder;

    public IContentSerializer ContentSerializer {
        get;
    } = contentSerializer;

    public IServiceProvider ServiceProvider {
        get;
    } = serviceProvider;

    public TChannelRequest? TransportRequest {
        get;
        set;
    }

    public TChannelResponse? TransportResponse {
        get;
        set;
    }
    
    public OperationRequest OperationRequest {
        get;
        protected set;
    } = operationRequest;

    public OperationResponse OperationResponse {
        get;
        protected set;
    } = operationResponse;

    public Task Next() {
        if (_filterIndex < operationFilters.Length) {
            return operationFilters[_filterIndex++].Invoke(this);
        }

        var transportIndex = _filterIndex - operationFilters.Length;
        
        if (transportIndex >= transportFilters.Length) {
            return Task.CompletedTask;
        }
        
        _filterIndex++;
        
        return transportFilters[transportIndex].Invoke(this);
    }
}