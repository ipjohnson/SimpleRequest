using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.ContextAccessor;

public interface IContextAccessor {
    IRequestContext? Context { get; set; }
}

[SingletonService(Realm = typeof(ContextAccessorSupport))]
public class SingletonContextAccessor : IContextAccessor {
    private readonly AsyncLocal<IRequestContext?> _context = new();
    
    public IRequestContext? Context {
        get => _context.Value;
        set => _context.Value = value;
    }
}