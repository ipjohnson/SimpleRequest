using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.SourceGenerator.Impl.Writers;

public class InvokeMethodGenerator {

    public void GenerateInvokeMethod(RequestHandlerModel requestModel, ClassDefinition classDefinition, ITypeDefinition requestParameterType) {
        var invokeMethod = classDefinition.AddMethod("Invoke");
        invokeMethod.Modifiers |= ComponentModifier.Private | ComponentModifier.Async | ComponentModifier.Static;

        var context = invokeMethod.AddParameter(KnownRequestTypes.IRequestContext, "context");
        invokeMethod.SetReturnType(typeof(Task));

        invokeMethod.AddUsingNamespace(KnownTypes.Microsoft.DependencyInjection.Namespace);

        var handler = invokeMethod.Assign(
            context.Property("ServiceProvider")
                .InvokeGeneric("GetRequiredService", new[] {
                    requestModel.HandlerType
                })
        ).ToVar("handler");
        
        var requestParameters = invokeMethod.Assign(context.Property("InvokeParameters")).ToVar("parameters");
        var generateParameterStatement = GetParameterStatements(requestModel, requestParameters);
        
        IOutputComponent invokeStatement = handler.Invoke(requestModel.HandlerMethod,
             generateParameterStatement.OfType<object>().ToArray());
        
        if (
            (requestModel.ResponseInformation.ReturnType?.Name?.StartsWith("Task") ?? false) &&
            (requestModel.ResponseInformation.ReturnType?.Namespace.Equals("System.Threading.Tasks") ?? false)) {
            invokeStatement = SyntaxHelpers.Await(invokeStatement);
        }

        if (TypeDefinition.Get(typeof(Task)).Equals(requestModel.ResponseInformation.ReturnType) || 
            TypeDefinition.Get(typeof(void)).Equals(requestModel.ResponseInformation.ReturnType)) {
            invokeMethod.AddIndentedStatement(invokeStatement);
        }
        else {
            invokeMethod.Assign(invokeStatement).To(context.Property("ResponseData").Property("ResponseValue"));
        }
    }

    private List<IOutputComponent> GetParameterStatements(RequestHandlerModel requestModel, InstanceDefinition requestParameters) {
        var returnList = new List<IOutputComponent>();

        foreach (var parameter in requestModel.RequestParameterInformationList) {
            returnList.Add(
                requestParameters.InvokeGeneric(
                    "Get",
                    new[] {
                        parameter.ParameterType
                    },
                    parameter.ParameterIndex));
        }

        return returnList;
    }
}