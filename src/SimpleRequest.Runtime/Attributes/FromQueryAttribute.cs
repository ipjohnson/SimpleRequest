namespace SimpleRequest.Runtime.Attributes;

public class FromQueryAttribute(string? name = null) : Attribute {
    public string? Name {
        get;
    } = name;
}