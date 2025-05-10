namespace SimpleRequest.Models.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class OperationsHandlerAttribute : Attribute {
    public string[]? Tags { get; set; }
}