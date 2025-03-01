namespace SimpleRequest.Swagger.Attributes;

public class GroupNameAttribute(string name) : Attribute {

    public string Name { get; } = name;
}