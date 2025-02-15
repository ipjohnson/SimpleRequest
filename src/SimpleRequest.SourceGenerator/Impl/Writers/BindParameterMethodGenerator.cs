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
        
        AssignToNewParameters(requestModel, parametersIf, requestParameterType, contextParameter, parameters, requestData);
        
        AssignToExistingParameters(requestModel, parametersIf, requestParameterType, contextParameter, parameters, requestData);
    }

    private void AssignToExistingParameters(
        RequestHandlerModel requestModel, IfElseLogicBlockDefinition parametersIf, ITypeDefinition requestParameterType, ParameterDefinition contextParameter, InstanceDefinition parameters, InstanceDefinition requestData) {
        var elseBlock = parametersIf.Else();
        
        for (var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
            var parameterInformation = requestModel.RequestParameterInformationList[i];

            IOutputComponent testStatement = NotEquals(
                parameters.Invoke("HasValue", parameterInformation.ParameterIndex),
                "true");
            if (parameterInformation.BindingType == ParameterBindType.Body) {
                testStatement = And(testStatement, 
                    NotEquals(
                        contextParameter.Property("RequestData").Property("Body"), 
                        Null()));
            }
            
            var ifBlock =
                elseBlock.If(testStatement);
            
            switch (parameterInformation.BindingType) {
                case ParameterBindType.Body:
                    BindBodyParameter(
                        ifBlock, contextParameter, requestData, parameters, parameterInformation, i);
                    break;
            }
        }
    }

    private void AssignToNewParameters(RequestHandlerModel requestModel, 
        IfElseLogicBlockDefinition parametersIf,
        ITypeDefinition requestParameterType,
        ParameterDefinition contextParameter,
        InstanceDefinition parameters, InstanceDefinition requestData) {
        
        parametersIf.Assign(New(requestParameterType)).To(parameters);
        parametersIf.Assign(parameters).To(contextParameter.Property("InvokeParameters"));
        
        for (var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
            var parameterInformation = requestModel.RequestParameterInformationList[i];
            
            switch (parameterInformation.BindingType) {
                case ParameterBindType.Body:
                    BindBodyParameter(
                        parametersIf, contextParameter, requestData, parameters, parameterInformation, i);
                    break;
            }
        }
    }

    private void BindBodyParameter(BaseBlockDefinition bindParameterInfoMethod,
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