namespace SimpleRequest.Models.Attributes;

public class RangeAttribute(object minValue, object maxValue) : Attribute {
    public object MinValue {
        get;
    } = minValue;

    public object MaxValue {
        get;
    } = maxValue;

}