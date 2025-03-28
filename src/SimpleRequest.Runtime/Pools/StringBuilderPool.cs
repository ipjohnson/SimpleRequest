using System.Text;
using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleRequest.Runtime.Pools;

public interface IStringBuilderPool : IItemPool<StringBuilder> { }

[SingletonService]
public class StringBuilderPool : ItemPool<StringBuilder>, IStringBuilderPool {
    [ActivatorUtilitiesConstructor]
    public StringBuilderPool() : this(2) { }

    public StringBuilderPool(int defaultSize)
        : base(() => new StringBuilder(defaultSize), b => b.Clear()) { }
}