using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Templates;

public interface ITemplateContextAware {
    IRequestContext RequestContext { get; set; }
}