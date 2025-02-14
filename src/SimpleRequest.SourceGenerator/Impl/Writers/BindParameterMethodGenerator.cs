using CSharpAuthor;
using SimpleRequest.SourceGenerator.Impl.Models;
using static CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.SourceGenerator.Impl.Writers;

public class BindParameterMethodGenerator {

    public void GenerateBindParameterMethod(RequestHandlerModel requestModel, ClassDefinition classDefinition, ITypeDefinition requestParameterType) {
        
        var bindParameterInfoMethod = classDefinition.AddMethod("BindParameterInfo");
        
        bindParameterInfoMethod.SetReturnType(TypeDefinition.Get(typeof(Task)));
        bindParameterInfoMethod.Modifiers |= 
            ComponentModifier.Static | ComponentModifier.Private | ComponentModifier.Async;
        
        var contextParameter = 
            bindParameterInfoMethod.AddParameter(KnownRequestTypes.IRequestContext, "context");

        GenerateParameterBindingLogic(
            requestModel, classDefinition, requestParameterType, bindParameterInfoMethod, contextParameter);
    }
    
    
    private void GenerateParameterBindingLogic(
        RequestHandlerModel requestModel, 
        ClassDefinition classDefinition, 
        ITypeDefinition requestParameterType, 
        MethodDefinition bindParameterInfoMethod, 
        ParameterDefinition contextParameter) {

        var requestData = 
            bindParameterInfoMethod.Assign(contextParameter.Property("RequestData")).ToVar("requestData");
        var parameters =
            bindParameterInfoMethod.Assign(contextParameter.Property("InvokeParameters")).ToVar("parameters");

        var parametersIf = 
            bindParameterInfoMethod.If(EqualsStatement(parameters, Null()));
        
        parametersIf.Assign(New(requestParameterType)).To(parameters);
        parametersIf.Assign(parameters).To(contextParameter.Property("InvokeParameters"));
        
        for (var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
            var parameterInformation = requestModel.RequestParameterInformationList[i];
            
            switch (parameterInformation.BindingType) {
                case ParameterBindType.Body:
                    BindBodyParameter(
                        bindParameterInfoMethod, contextParameter, requestData, parameters, parameterInformation, i);
                    break;
            }
        }
    }

    private void BindBodyParameter(MethodDefinition bindParameterInfoMethod,
        ParameterDefinition contextParameter,
        InstanceDefinition requestData,
        InstanceDefinition parameters,
        RequestParameterInformation parameterInformation,
        int index) {
        var invokeStatement =
            contextParameter.Property("ContentSerializerManager").InvokeGeneric(
                "Deserialize",
                new []{
                    parameterInformation.ParameterType
                },
                contextParameter
                );
        
        var invoke = 
            parameters.Invoke("Set", Await(invokeStatement), index);

        bindParameterInfoMethod.AddUsingNamespace(KnownRequestTypes.SerializersNamespace);
        bindParameterInfoMethod.AddIndentedStatement(invoke);
    }
}