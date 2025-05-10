namespace SimpleRequest.Models.Attributes;

public class MaxValueAttribute(object maxValue) : Attribute {
    public object MaxValue {
        get;
    } = maxValue;

}