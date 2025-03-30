using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.StaticContent;

public interface IStaticContentEnhancement {
    ValueTask EnhanceRequest(IRequestContext context);
}