using DependencyModules.Runtime.Attributes;
using SimpleRequest.Client.Filters;
using SimpleRequest.Client.Serialization;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Impl;

public delegate Task<T> InvokeDelegate<T>(
    IServiceProvider serviceProvider, IOperationParameters parameters);

public interface IInvokeStrategyBuilder<TRequest,TResponse> {
    InvokeDelegate<T?> Build<T>(string channelName, IOperationInfo operation, IContentSerializer serializer);
}

[SingletonService]
public class InvokeStrategyBuilder<TRequest,TResponse> : IInvokeStrategyBuilder<TRequest,TResponse> {
    private readonly IOperationInvokeHandler<TRequest, TResponse> _handler;
    private readonly IReadOnlyList<ITransportInvokeFilter<TRequest, TResponse>> _transportInvokeFilters;
    private readonly IReadOnlyList<ITransportBindFilter<TRequest, TResponse>> _transportBindFilters;
    private readonly IReadOnlyList<ITransportFilter<TRequest, TResponse>> _transportFilter;
    private readonly IReadOnlyList<IOperationFilter> _operationFilters;

    public InvokeStrategyBuilder(
        IOperationInvokeHandler<TRequest, TResponse> handler,
        IEnumerable<IOperationFilter> operationFilters,
        IEnumerable<ITransportFilter<TRequest, TResponse>> transportFilter,
        IEnumerable<ITransportInvokeFilter<TRequest, TResponse>> transportInvokeFilters,
        IEnumerable<ITransportBindFilter<TRequest, TResponse>> transportBindFilters) {
        _handler = handler;
        _transportInvokeFilters = transportInvokeFilters.Reverse().ToList();
        _transportBindFilters = transportBindFilters.Reverse().ToList();
        _transportFilter = transportFilter.Reverse().ToList();
        _operationFilters = operationFilters.Reverse().ToList();
    }
    
    public InvokeDelegate<T?> Build<T>(string channelName, IOperationInfo operation, IContentSerializer serializer) {
        var operationFilters = 
            _operationFilters.Where(
                o => o.SupportOperation(channelName, operation)).ToList();
        
        var transportFilters = 
            _transportFilter.Where(
                t => t.SupportOperation(channelName, operation)).ToList();

        var transportInvokeFilter = 
            _transportInvokeFilters.FirstOrDefault(
                t => t.SupportOperation(channelName, operation));
        
        var bindingInvokeFilter = 
            _transportBindFilters.FirstOrDefault(
                t => t.SupportOperation(channelName, operation));

        if (transportInvokeFilter != null) {
            transportFilters.Add(transportInvokeFilter);
        }

        if (bindingInvokeFilter != null) {
            transportFilters.Add(bindingInvokeFilter);
        }
        
        transportFilters.Sort(
            (x, y) => Comparer<int>.Default.Compare(x.Order, y.Order));

        operationFilters.Sort(
            (x, y) => Comparer<int>.Default.Compare(x.Order, y.Order));
        
        var invokeInfo = new InvokeDelegateInfo<TRequest, TResponse>(
            channelName,
            operation,
            new PathBuilder(operation),
            serializer,
            operationFilters.ToArray(),
            transportFilters.ToArray()
        );
        
        return (provider, parameters) => _handler.InvokeDelegate<T>(
            invokeInfo,
            provider,
            parameters
        );
    }
}