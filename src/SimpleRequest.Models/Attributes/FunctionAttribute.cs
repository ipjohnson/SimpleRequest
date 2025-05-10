namespace SimpleRequest.Models.Attributes;

public class FunctionAttribute : BaseFunctionAttribute {
    public string? Name { get; set; }
    
    public string[]? Tags { get; set; }
}