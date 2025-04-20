namespace SimpleRequest.Models.Attributes;

public class FromQueryAttribute(string name = "") : Attribute {
    public string Name {
        get;
    } = name;
}