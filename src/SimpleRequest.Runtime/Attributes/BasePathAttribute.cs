namespace SimpleRequest.Runtime.Attributes;

public class BasePathAttribute : Attribute {
    public BasePathAttribute(string path) {
        Path = path;
    }

    public string Path { get; }
}