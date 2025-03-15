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

        var bindParameterService =
            bindParameterInfoMethod.Assign(
                contextParameter.Property("RequestServices").Property("BindingService")).ToVar("bindingService");

        var parameters =
            bindParameterInfoMethod.Assign(contextParameter.Property("InvokeParameters")).ToVar("parameters");

        var parametersIf =
            bindParameterInfoMethod.If(EqualsStatement(parameters, Null()));

        AssignToNewParameters(requestModel, parametersIf, requestParameterType, contextParameter, parameters, bindParameterService);

        AssignToExistingParameters(requestModel, parametersIf, requestParameterType, contextParameter, parameters, bindParameterService);
    }

    private void AssignToExistingParameters(
        RequestHandlerModel requestModel,
        IfElseLogicBlockDefinition parametersIf,
        ITypeDefinition requestParameterType,
        ParameterDefinition contextParameter,
        InstanceDefinition parameters,
        InstanceDefinition bindParameterService) {
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

            BindParameter(
                ifBlock, contextParameter, parameters, bindParameterService,  parameterInformation, i);
        }
    }

    private void BindPathParameter(
        IfElseLogicBlockDefinition block,
        ParameterDefinition contextParameter,
        InstanceDefinition bindingService,
        InstanceDefinition parameters,
        RequestParameterInformation parameterInformation, int i) {
        var tokenStatement = contextParameter.Property("RequestData")
            .Property("PathTokenCollection")
            .Invoke("Get", QuoteString(parameterInformation.Name));

        var invoke = bindingService.Invoke(
            "BindValue",
            parameters,
            GetStaticparams(parameterInformation),
            contextParameter,
            tokenStatement
        );

        block.AddIndentedStatement(invoke);
    }

    private static string GetStaticparams(RequestParameterInformation parameterInformation) {
        return "InvokeParameters.StaticParams.static_" + parameterInformation.Name;
    }

    private void AssignToNewParameters(RequestHandlerModel requestModel,
        IfElseLogicBlockDefinition parametersIf,
        ITypeDefinition requestParameterType,
        ParameterDefinition contextParameter,
        InstanceDefinition parameters,
        InstanceDefinition requestData) {

        parametersIf.Assign(New(requestParameterType)).To(parameters);
        parametersIf.Assign(parameters).To(contextParameter.Property("InvokeParameters"));

        for (var i = 0; i < requestModel.RequestParameterInformationList.Count; i++) {
            var parameterInformation = requestModel.RequestParameterInformationList[i];

            BindParameter(parametersIf, contextParameter, parameters, requestData, parameterInformation, i);
        }
    }

    private void BindParameter(IfElseLogicBlockDefinition parametersIf, ParameterDefinition contextParameter, InstanceDefinition parameters, InstanceDefinition requestData,
        RequestParameterInformation parameterInformation, int i) {
        switch (parameterInformation.BindingType) {
            case ParameterBindType.Body:
                BindBodyParameter(
                    parametersIf, contextParameter, requestData, parameters, parameterInformation);
                break;
            case ParameterBindType.Path:
                BindPathParameter(
                    parametersIf, contextParameter, requestData, parameters, parameterInformation, i);
                break;
            case ParameterBindType.CustomAttribute:
                BindCustomParameter(parametersIf, contextParameter, requestData, parameters, parameterInformation, i);
                break;
            case ParameterBindType.RequestContext:
                BindContext(parametersIf, contextParameter, requestData, parameters, parameterInformation, i);
                break;
            case ParameterBindType.Header:
                BindRequestValue("Headers", parametersIf, contextParameter, requestData, parameters, parameterInformation);
                break;
            case ParameterBindType.QueryString:
                BindRequestValue("QueryParameters", parametersIf, contextParameter, requestData, parameters, parameterInformation);
                break;
            case ParameterBindType.Cookie:
                BindRequestValue("Cookies", parametersIf, contextParameter, requestData, parameters, parameterInformation);
                break;
        }
    }

    
    private void BindRequestValue(string collectionName,
        IfElseLogicBlockDefinition block,
        ParameterDefinition contextParameter, 
        InstanceDefinition bindingService,
        InstanceDefinition parameters,
        RequestParameterInformation parameterInformation) {
        var tokenStatement = contextParameter.Property("RequestData")
            .Property(collectionName)
            .Invoke("GetValueOrDefault", QuoteString(parameterInformation.BindingName));

        tokenStatement.AddUsingNamespace("SimpleRequest.Runtime.Utilities");
        
        var invoke = bindingService.Invoke(
            "BindValue",
            parameters,
            GetStaticparams(parameterInformation),
            contextParameter,
            tokenStatement
        );

        block.AddIndentedStatement(invoke);
    }

    private void BindContext(IfElseLogicBlockDefinition parametersIf, ParameterDefinition contextParameter, InstanceDefinition requestData, InstanceDefinition parameters,
        RequestParameterInformation parameterInformation, int i) {
        var invoke =
            parameters.Invoke("Set", contextParameter, i);

        parametersIf.AddIndentedStatement(invoke);
    }

    private void BindCustomParameter(
        IfElseLogicBlockDefinition parametersIf,
        ParameterDefinition contextParameter,
        InstanceDefinition bindingParameter,
        InstanceDefinition parameters,
        RequestParameterInformation parameterInformation,
        int i) {
        var bindInvoke = bindingParameter.Invoke(
            "BindCustomAttribute",
            parameters,
            GetStaticparams(parameterInformation),
            contextParameter);

        parametersIf.AddIndentedStatement(Await(bindInvoke));
    }

    private void BindBodyParameter(BaseBlockDefinition bindParameterInfoMethod,
        ParameterDefinition contextParameter,
        InstanceDefinition bindParameterService,
        InstanceDefinition parameters,
        RequestParameterInformation parameterInformation) {

        var invokeStatement = bindParameterService.InvokeGeneric("BindBody", new[] {
                parameterInformation.ParameterType
            },
            parameters,
            GetStaticparams(parameterInformation),
            contextParameter
        );

        bindParameterInfoMethod.AddIndentedStatement(
            Await(invokeStatement)
        );
    }
}