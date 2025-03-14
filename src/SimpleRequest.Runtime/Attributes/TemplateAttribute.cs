namespace SimpleRequest.Runtime.Attributes;

public class TemplateAttribute(string templateName) : Attribute {
    public string TemplateName {
        get;
    } = templateName;
}