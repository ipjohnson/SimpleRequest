namespace SimpleRequest.Models.Attributes;

public abstract class BaseFunctionAttribute : Attribute {
    
    public int SuccessStatus { get; set; } = 200;

    public int ValidationErrorStatus { get; set; } = 400;

    public int ErrorStatus { get; set; } = 500;
}