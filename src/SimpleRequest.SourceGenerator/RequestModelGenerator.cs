using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl;
using SimpleRequest.SourceGenerator.Impl.Handler;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.SourceGenerator;

public class RequestModelGenerator : BaseRequestModelGenerator {
    private static string[] _attributeNames = {
        "Get",
        "GetAttribute",   
        "Post",
        "PostAttribute",
        "Put",
        "PutAttribute",
        "Delete",
        "DeleteAttribute",
        "Function",
        "FunctionAttribute",
        "Http",
        "HttpAttribute",
    };
    
    protected override RequestHandlerNameModel GetRequestNameModel(GeneratorSyntaxContext context,
        MethodDeclarationSyntax methodDeclaration,
        IReadOnlyList<AttributeModel> attributeModels,
        IReadOnlyList<AttributeModel> classAttributes,
        CancellationToken cancellation) {

        var method = "POST";
        var functionName = methodDeclaration.Identifier.Text;
        var attribute = attributeModels.FirstOrDefault(a => _attributeNames.Contains(a.TypeDefinition.Name));

        if (attribute != null) {
            if (attribute.TypeDefinition.Equals(KnownRequestTypes.Attributes.Function)) {
                functionName = attribute.Properties.FirstOrDefault()?.Value?.ToString() ?? functionName;
            }
            else if (attribute.TypeDefinition.Equals(KnownRequestTypes.Attributes.Http)) {
                method = attribute.Arguments.FirstOrDefault()?.Value?.ToString() ?? "POST";
                functionName = attribute.Arguments.LastOrDefault()?.Value?.ToString() ?? "/";
            }
            else {
                method = attribute.TypeDefinition.Name.Replace("Attribute", "").ToUpperInvariant();
                functionName = attribute.Arguments.FirstOrDefault()?.Value?.ToString() ?? "/";
            }
        }

        if (string.IsNullOrEmpty(functionName)) {
            functionName = "/";
        }
        
        var basePath = classAttributes.FirstOrDefault(
                a => a.TypeDefinition.Name == "BasePathAttribute")?
            .Arguments.FirstOrDefault()?.Value?.ToString().Trim('"');
        
        functionName = basePath + functionName;
        
        return new RequestHandlerNameModel(functionName, method);
    }

    protected override RequestParameterInformation? GetParameterInfoFromAttributes(
        GeneratorSyntaxContext generatorSyntaxContext,
        MethodDeclarationSyntax methodDeclarationSyntax,
        RequestHandlerNameModel requestHandlerNameModel,
        ParameterSyntax parameter, 
        IReadOnlyList<AttributeModel> attributeModels,
        int parameterIndex) {

        return null;
    }

    protected override IEnumerable<string> AttributeNames() => _attributeNames;
    
}