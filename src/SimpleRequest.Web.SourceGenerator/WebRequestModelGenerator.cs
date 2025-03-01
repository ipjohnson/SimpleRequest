using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Handler;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.Web.SourceGenerator;

public class WebRequestModelGenerator : BaseRequestModelGenerator {
    private static string[] _attributeNames = {
        "Get",
        "GetAttribute",   
        "Post",
        "PostAttribute",
        "Put",
        "PutAttribute",
        "Delete",
        "DeleteAttribute"
    };
    
    protected override RequestHandlerNameModel GetRequestNameModel(GeneratorSyntaxContext context,
        MethodDeclarationSyntax methodDeclaration,
        IReadOnlyList<AttributeModel> attributeModels,
        CancellationToken cancellation) {

        var method = "POST";
        var functionName = methodDeclaration.Identifier.Text;
        var attribute = attributeModels.FirstOrDefault(a => _attributeNames.Contains(a.TypeDefinition.Name));

        if (attribute != null) {
            method = attribute.TypeDefinition.Name.Replace("Attribute", "").ToUpperInvariant();
            functionName = attribute.Arguments.FirstOrDefault()?.Value?.ToString() ?? "/";
        }

        if (string.IsNullOrEmpty(functionName)) {
            functionName = "/";
        }
        
        return new RequestHandlerNameModel(functionName, method);
    }

    protected override RequestParameterInformation? GetParameterInfoFromAttributes(
        GeneratorSyntaxContext generatorSyntaxContext, MethodDeclarationSyntax methodDeclarationSyntax, RequestHandlerNameModel requestHandlerNameModel,
        ParameterSyntax parameter, int parameterIndex) {
        foreach (var attributeList in parameter.AttributeLists) {
            foreach (var attribute in attributeList.Attributes) {
               
                var attributeModel = AttributeModelHelper.GetAttribute(generatorSyntaxContext, attribute);
            }
        }

        return null;
    }

    protected override IEnumerable<string> AttributeNames() => _attributeNames;

    private static RequestParameterInformation GetParameterInfoWithBinding(
        GeneratorSyntaxContext generatorSyntaxContext,
        ParameterSyntax parameter,
        ParameterBindType bindingType,
        string bindingName,
        int parameterIndex) {
        var parameterType = parameter.Type?.GetTypeDefinition(generatorSyntaxContext)!;

        return CreateRequestParameterInformation(parameter,
            parameterType,
            bindingType,
            parameterIndex,
            null,
            bindingName);
    }

}