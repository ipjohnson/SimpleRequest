namespace SimpleRequest.Models.Attributes;

public class BasePathAttribute(string path) : Attribute {
    public string Path {
        get;
    } = path;
}