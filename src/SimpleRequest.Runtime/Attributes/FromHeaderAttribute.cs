namespace SimpleRequest.Runtime.Attributes;

public class FromHeaderAttribute(string? name = null) : Attribute {

    public string? Name { get; } = name;
}