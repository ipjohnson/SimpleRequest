namespace SimpleRequest.Models.Operations;

public enum ParameterBindType {
    Path,
    QueryString,
    Header,
    Cookie,
    Body,
    FromServiceProvider,
    CustomAttribute,
    FrameworkType,
    Other
}

public interface IOperationParameterInfo {
    string Name { get; }

    int Index { get; }

    Type Type { get; }
    
    string BindingName { get; }
    
    ParameterBindType BindingType { get; }
    
    object? DefaultValue { get; }
    
    bool IsRequired { get; }
    
    IReadOnlyList<Attribute> Attributes { get; }
    
    void InvokeGenericCapture(IGenericParameterCapture receiver);
}

public interface IGenericParameterCapture {
    void Capture<T>(IOperationParameterInfo parameterInfo);
}