namespace SimpleRequest.Runtime.Invoke;

public interface IInvokeParameterValueProvider {
    ValueTask<object?> GetParameterValueAsync(IRequestContext context, IInvokeParameterInfo parameter);
}