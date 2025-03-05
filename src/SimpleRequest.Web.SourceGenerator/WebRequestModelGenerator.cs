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
        GeneratorSyntaxContext generatorSyntaxContext,
        MethodDeclarationSyntax methodDeclarationSyntax,
        RequestHandlerNameModel requestHandlerNameModel,
        ParameterSyntax parameter, 
        IReadOnlyList<AttributeModel> attributeModels,
        int parameterIndex) {

        foreach (var attributeModel in attributeModels) {
            if (attributeModel.TypeDefinition.Name == "FromHeaderAttribute") {
                var argument = attributeModel.Arguments.FirstOrDefault()?.Value?.ToString();
                
                var headerName = string.IsNullOrEmpty(argument) ? parameter.Identifier.ToString() : argument;
                return GetParameterInfoWithBinding(
                    generatorSyntaxContext,
                    parameter,ParameterBindType.Header, 
                    headerName!, 
                    parameterIndex);
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
        
        if (!parameterType.IsNullable && parameter.ToFullString().Contains("?")) {
            parameterType = parameterType.MakeNullable();
        }

        var attributeModels = 
            AttributeModelHelper.GetAttributeModels(generatorSyntaxContext, parameter, CancellationToken.None);
        
        return CreateRequestParameterInformation(parameter,
            parameterType,
            bindingType,
            parameterIndex,
            parameterType.IsNullable,
            bindingName,
            attributeModels);
    }

}