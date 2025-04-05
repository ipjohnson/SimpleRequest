using System.Text.Json;
using System.Text.Json.Nodes;
using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Runtime.Serializers.Json;

namespace SimpleRequest.JsonRpc.Impl;

public interface IJsonRpcParameterBinder {
    bool TryBind(IRequestContext context, JsonNode? parameters);
}

[SingletonService]
public class JsonRpcParameterBinder(ISystemTextJsonSerializerOptionProvider serializerOptionProvider) : IJsonRpcParameterBinder {

    public bool TryBind(IRequestContext context, JsonNode? parameters) {
        if (context.RequestHandlerInfo == null) {
            return false;
        }

        if (context.RequestHandlerInfo.InvokeInfo.Parameters.Count == 0) {
            return IsEmptyParameters(parameters);
        }

        if (parameters == null) {
            return false;
        }

        var invokeParameters =
            context.RequestHandlerInfo.InvokeInfo.CreateParameters(context);

        context.InvokeParameters = invokeParameters;

        if (parameters.GetValueKind() == JsonValueKind.Object) {
            return ProcessObjectParameters(context, invokeParameters, parameters.AsObject());
        }

        if (parameters.GetValueKind() == JsonValueKind.Array) {
            return ProcessSlottedParameters(context, invokeParameters, parameters.AsArray());
        }

        return false;
    }

    private static bool IsEmptyParameters(JsonNode? parameters) {
        return parameters == null;
    }

    private bool ProcessObjectParameters(IRequestContext context, IInvokeParameters invokeParameters, JsonObject parameters) {

        for (var i = 0; i < context.RequestHandlerInfo!.InvokeInfo.Parameters.Count; i++) {
            var parameter = context.RequestHandlerInfo.InvokeInfo.Parameters[i];

            if (!parameters.TryGetPropertyValue(parameter.Name, out var value)) {
                if (parameter.IsRequired) {
                    return false;
                }
            }

            if (value == null) {
                if (parameter.IsRequired) {
                    return false;
                }
            }
            else if (!AssignParameter(invokeParameters, parameter, value)) {
                return false;
            }
        }

        return true;
    }

    private bool ProcessSlottedParameters(IRequestContext context, IInvokeParameters invokeParameters, JsonArray parameters) {

        for (var i = 0; i < context.RequestHandlerInfo!.InvokeInfo.Parameters.Count; i++) {
            var parameter = context.RequestHandlerInfo.InvokeInfo.Parameters[i];

            if (parameters.Count < i) {
                if (parameter.IsRequired) {
                    return false;
                }
            }
            else {
                var jsonNode = parameters[i];

                if (jsonNode == null) {
                    if (parameter.IsRequired) {
                        return false;
                    }
                }
                else if (!AssignParameter(invokeParameters, parameter, jsonNode)) {
                    return false;
                }
            }
        }

        return true;
    }

    private bool AssignParameter(IInvokeParameters invokeParameters, IInvokeParameterInfo parameter, JsonNode jsonNode) {
        if (jsonNode.GetValueKind() == JsonValueKind.Null) {
            return !parameter.IsRequired;
        }

        try {
            parameter.InvokeGenericCapture(
                new JsonRpcGenericParameterReceiver(invokeParameters, parameter, jsonNode, serializerOptionProvider.GetOptions())
            );
        }
        catch (Exception e) {
            return false;
        }

        return true;
    }

    private class JsonRpcGenericParameterReceiver(
        IInvokeParameters invokeParameters,
        IInvokeParameterInfo parameter,
        JsonNode jsonNode,
        JsonSerializerOptions options) : IGenericParameterCapture {

        public void Capture<T>(IInvokeParameterInfo parameterInfo) {
            var value = jsonNode.Deserialize(typeof(T), options);

            invokeParameters.Set(value, parameter.Index);
        }
    }
}