namespace SimpleRequest.Runtime.Attributes;

public class CompressResponseAttribute(bool enable = true) : Attribute {
    public bool Enable { get; } = enable;
}