namespace SimpleRequest.Runtime.Invoke.Impl;


public class RequestChain : IRequestChain {
    
    private readonly IReadOnlyList<FilterProvider> _filterChain;
    private int _index;

    public RequestChain(IReadOnlyList<FilterProvider> filterChain,
        IRequestContext context) {
        _filterChain = filterChain;
        Context = context;
    }

    private RequestChain(IReadOnlyList<FilterProvider> filterChain,
        IRequestContext context, int index) {
        _index = index;
        _filterChain = filterChain;
        Context = context;
    }

    public Task Next() {
        if (_index > _filterChain.Count) {
            return Task.CompletedTask;
        }

        return _filterChain[_index++](Context).Invoke(this);
    }

    public IRequestContext Context { get; }

    public IRequestChain Fork(IRequestContext? context) {
        return new RequestChain(_filterChain, context ?? Context, _index);
    }

    public bool IsLastFilter => _index >= _filterChain.Count;
}