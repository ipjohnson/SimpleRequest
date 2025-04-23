namespace SimpleRequest.Models.Attributes;

public class FromCookieAttribute(string name = "") : Attribute {
    public string Name { get; } = name;
}