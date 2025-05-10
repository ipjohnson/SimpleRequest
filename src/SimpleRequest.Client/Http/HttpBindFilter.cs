using DependencyModules.Runtime.Attributes;
using SimpleRequest.Client.Filters;
using SimpleRequest.Models.Attributes;
using SimpleRequest.Models.Operations;

namespace SimpleRequest.Client.Http;

[SingletonService]
public class HttpBindFilter(
    IHttpMessageFactory httpMessageFactory,
    IHttpHeaderBinder headerBinder,
    IHttpRequestBodyBinder requestBodyBinder,
    IHttpResponseBodyBinder responseBodyBinder) : ITransportBindFilter<HttpRequestMessage, HttpResponseMessage> {
    public int Order => 1_000_000;

    public bool SupportOperation(string channel, IOperationInfo operation) {
        return true;
    }

    public async Task Invoke(ITransportFilterContext<HttpRequestMessage, HttpResponseMessage> context) {
        await BindParameters(context);

        try {
            await context.Next();
            
            await BindResponse(context);
        }
        catch (Exception ex) {
            context.OperationResponse.Exception = ex;
        }
    }

    protected virtual Task BindResponse(ITransportFilterContext<HttpRequestMessage,HttpResponseMessage> context) {
        return responseBodyBinder.BindResponse(context);
    }

    protected virtual async Task BindParameters(ITransportFilterContext<HttpRequestMessage,HttpResponseMessage> context) {
        var parameters = context.OperationRequest.Parameters;

        var httpMessage = 
            context.TransportRequest ??= httpMessageFactory.GenerateRequestMessage(context);

        for (int i = 0; i < context.OperationRequest.Operation.Attributes.Count; i++) {
            var attribute = context.OperationRequest.Operation.Attributes[i];

            if (attribute is IHttpAttribute httpAttribute) {
                httpMessage.Method = httpAttribute.HttpMethod;
            }
        }
        
        for (var i = 0; i < parameters.ParameterCount; i++) {
            var parameterInstance = parameters.Parameters[i];
            
            switch (parameterInstance.BindingType) {
                case ParameterBindType.Body:
                    await requestBodyBinder.BindBody(context, parameterInstance);
                    break;
                case ParameterBindType.Header:
                    
                    break;
                case ParameterBindType.QueryString:
                    break;
                case ParameterBindType.Cookie:
                    break;
            }
        }
    }


}