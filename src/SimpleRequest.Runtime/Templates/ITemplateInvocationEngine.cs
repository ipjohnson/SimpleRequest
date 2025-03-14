using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Templates;

public delegate Task TemplateInvocationDelegate(IRequestContext context);

public interface ITemplateInvocationEngine {
    Task Invoke(IRequestContext context);
    
    TemplateInvocationDelegate? GetInvocationDelegate(string templateName);
}