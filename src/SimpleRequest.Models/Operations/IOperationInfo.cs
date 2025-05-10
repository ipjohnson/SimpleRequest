namespace SimpleRequest.Models.Operations;

public interface IOperationInfo {
    string Path { get; }

    string Method { get; }

    Type HandlerType { get; }

    int? SuccessStatus { get; }
    
    int? FailureStatus  { get; }

    int? NullResponseStatus  { get; }
    
    IReadOnlyList<Attribute> Attributes { get; }
    
    IReadOnlyList<IOperationParameterInfo> Parameters { get; }
    
    Type ResponseType { get; }
}