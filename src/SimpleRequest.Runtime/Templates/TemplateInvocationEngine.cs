using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Templates;

[SingletonService]
public class TemplateInvocationEngine : ITemplateInvocationEngine {
    private readonly Dictionary<string, TemplateInfo> _templates;

    public TemplateInvocationEngine(IEnumerable<ITemplateProvider> templateProviders) {
        _templates = new Dictionary<string, TemplateInfo>();
        foreach (var templateProvider in templateProviders) {
            foreach (var templateInfo in templateProvider.GetTemplates()) {
                _templates[templateInfo.TemplateName] = templateInfo;
            }
        }
    }
    
    public Task Invoke(IRequestContext context) {
        if (string.IsNullOrEmpty(context.ResponseData.TemplateName)) {
            return Task.CompletedTask;
        }
        var templateDelegate = GetInvocationDelegate(context.ResponseData.TemplateName);

        if (templateDelegate is null) {
            throw new Exception("Template not found: " + context.ResponseData.TemplateName);
        }
        
        return templateDelegate(context);
    }

    public TemplateInvocationDelegate? GetInvocationDelegate(string templateName) {
        return _templates.GetValueOrDefault(templateName)?.InvocationDelegate;
    }
}