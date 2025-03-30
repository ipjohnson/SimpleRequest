namespace SimpleRequest.Runtime.Templates;

public record TemplateInfo(
    string TemplateName,
    TemplateInvocationDelegate InvocationDelegate
    );

public interface ITemplateProvider {
    IEnumerable<TemplateInfo> GetTemplates();
}