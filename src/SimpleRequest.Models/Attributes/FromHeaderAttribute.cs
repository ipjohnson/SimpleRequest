namespace SimpleRequest.Models.Attributes;

public class FromHeaderAttribute(string name = "") : Attribute {
    public string Name { get; } = name;
}