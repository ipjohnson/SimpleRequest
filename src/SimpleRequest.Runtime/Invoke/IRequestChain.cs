namespace SimpleRequest.Runtime.Invoke;

public interface IRequestChain {
    /// <summary>
    /// Invoke next in chain
    /// </summary>
    /// <returns></returns>
    Task Next();
    
    /// <summary>
    /// Current context
    /// </summary>
    IRequestContext Context { get; }
    
    /// <summary>
    /// Fork request chain
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    IRequestChain Fork(IRequestContext? context = null);
    
    /// <summary>
    /// Returns true if there are no more filters in chain
    /// </summary>
    bool IsLastFilter { get; }
}