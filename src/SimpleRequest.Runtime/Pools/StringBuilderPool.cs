using System.Text;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.Runtime.Pools;

public interface IStringBuilderPool : IItemPool<StringBuilder> { }

[SingletonService]
public class StringBuilderPool : ItemPool<StringBuilder>, IStringBuilderPool {
    public StringBuilderPool() : this(2) { }

    public StringBuilderPool(int defaultSize)
        : base(() => new StringBuilder(defaultSize), b => b.Clear()) { }
}