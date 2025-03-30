using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.StaticContent;

public interface IStaticContentManager {
    Task SendContent(IRequestContext context, StaticContentInfo contentInfo, Func<IRequestContext,StaticContentInfo, Task> writeContent);
}

public class StaticContentManager : IStaticContentManager {
    private readonly IReadOnlyList<IStaticContentEnhancement> _enhancements;

    public StaticContentManager(IEnumerable<IStaticContentEnhancement> enhancements) {
        _enhancements = enhancements.ToArray();
    }

    public async Task SendContent(IRequestContext context, StaticContentInfo contentInfo, Func<IRequestContext, StaticContentInfo, Task> writeContent) {
        foreach (var enhancement in _enhancements) {
            await enhancement.EnhanceRequest(context);
        }
        
        await writeContent(context, contentInfo);
    }
}