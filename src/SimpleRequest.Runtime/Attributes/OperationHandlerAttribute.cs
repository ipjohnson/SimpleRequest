namespace SimpleRequest.Runtime.Attributes;

/// <summary>
/// Map operations to one method, all method marked with this method
/// must have an argument of IRequestContext and handle all data binding
/// </summary>
/// <typeparam name="T"></typeparam>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class OperationHandlerAttribute<T> : Attribute {
    
}