namespace SimpleRequest.Runtime.Attributes;

public class ContentTypeAttribute(string contentType) : Attribute {
    public string ContentType { get; } = contentType;
}