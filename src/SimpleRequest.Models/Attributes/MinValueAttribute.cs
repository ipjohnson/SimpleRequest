namespace SimpleRequest.Models.Attributes;

public class MinValueAttribute(object minValue) : Attribute {
    public object MinValue {
        get;
    } = minValue;
}