using DependencyModules.Runtime.Attributes;
using Microsoft.Extensions.Primitives;
using SimpleRequest.Runtime.Exceptions;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Runtime.Serializers;

public interface IParameterBindingService {
    void BindValue(IInvokeParameters parameters, IInvokeParameterInfo parameterInfo, IRequestContext context, object? parameterValue);
    
    Task BindBody<TModel>(IInvokeParameters parameters, IInvokeParameterInfo parameterInfo, IRequestContext context);
    
    Task BindCustomAttribute(IInvokeParameters parameters, IInvokeParameterInfo parameterInfo, IRequestContext context);
}

[SingletonService]
public class ParameterBindingService(IContentSerializerManager contentSerializerManager) : IParameterBindingService {

    public void BindValue(IInvokeParameters parameters,
        IInvokeParameterInfo parameterInfo,
        IRequestContext context, 
        object? parameterValue) {
        parameterValue ??= parameterInfo.DefaultValue;

        if (parameterValue == null) {
            if (parameterInfo.IsRequired) {
                throw new MissingValueValidationException(parameterInfo.Name);
            }
            
            return;
        }

        object? value = null;

        if (parameterValue is string s && parameterInfo.Type == typeof(string)) {
            value = s;
        } else if (parameterValue is StringValues stringValues && parameterInfo.Type == typeof(string)) {
            value = stringValues.ToString();
        }
        else {
            value = parameterValue.GetType().IsAssignableTo(parameterInfo.Type) ? 
                parameterValue : Convert.ChangeType(value, parameterInfo.Type);
        }
        
        if (value != null ) {
            parameters.Set(value, parameterInfo.Index);
        } 
        else if (parameterInfo.IsRequired)
        {
            throw new MissingValueValidationException(parameterInfo.Name);
        }
    }

    public async Task BindBody<TModel>(IInvokeParameters parameters, IInvokeParameterInfo parameterInfo, IRequestContext context) {
        object? value = null;
        
        if (context.RequestData.Body != null) {
            value = await contentSerializerManager.Deserialize<TModel>(context);
        }
        
        if (value == null && parameterInfo.IsRequired) {
            throw new MissingValueValidationException("http body");
        }

        parameters.Set(value, parameterInfo.Index);
    }

    public async Task BindCustomAttribute(IInvokeParameters parameters, IInvokeParameterInfo parameterInfo, IRequestContext context) {
        foreach (var attribute in parameterInfo.Attributes) {
            if (attribute is IInvokeParameterValueProvider provider) {
                var parameterValue = await provider.GetParameterValueAsync(context, parameterInfo);

                if (parameterValue != null) {
                    parameters.Set(parameterInfo.Name, parameterInfo.Index);
                    return;
                }
            }
        }

        if (parameterInfo.IsRequired) {
            throw new MissingValueValidationException(parameterInfo.Name);
        }
    }
}