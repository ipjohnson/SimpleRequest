namespace SimpleRequest.Models.Attributes;

public class OperationIdAttribute(string operationId) : Attribute {
    public string OperationId {
        get;
    } = operationId;
}