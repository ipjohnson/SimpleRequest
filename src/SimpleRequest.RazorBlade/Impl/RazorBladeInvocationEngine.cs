using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Templates;
    
namespace SimpleRequest.RazorBlade.Impl;

public delegate Task RazorBladeDelegate(TextWriter textWriter, CancellationToken cancellationToken);

public interface IRazorBladeInvocationEngine {

    TemplateInvocationDelegate CreateInvocationDelegate(Func<IRequestContext, IRazorBladeTemplate> templateFactory);
}

[SingletonService]
public class RazorBladeInvocationEngine : IRazorBladeInvocationEngine {
    public TemplateInvocationDelegate CreateInvocationDelegate(Func<IRequestContext, IRazorBladeTemplate> templateFactory) {
        return context => Invoke(templateFactory, context);
    }

    private async Task Invoke(Func<IRequestContext, IRazorBladeTemplate> templateFactory, IRequestContext context) {
        if (context.ResponseData.Body == null) {
            return;
        }
        
        var template = templateFactory(context);

        if (template is ITemplateContextAware contextAware) {
            contextAware.RequestContext = context;
        }
        
        context.ResponseData.ContentType = "text/html";
        var streamWrite = new StreamWriter(context.ResponseData.Body);
        
        await template.RenderAsync(streamWrite, context.CancellationToken);
        
        await streamWrite.FlushAsync();
    }
}