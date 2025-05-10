using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Handler;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.JsonRpc.SourceGenerator.Impl;

public class JsonRpcModelGenerator : BaseRequestModelGenerator {
    private static readonly string[] _attributeNames = {
        "JsonRpcFunction", "JsonRpcFunctionAttribute"
    };


    protected override RequestHandlerNameModel GetRequestNameModel(
        GeneratorSyntaxContext context,
        MethodDeclarationSyntax methodDeclaration,
        IReadOnlyList<AttributeModel> attributeModels,
        IReadOnlyList<AttributeModel> classAttributes,
        CancellationToken cancellation) {
        var functionName = methodDeclaration.Identifier.Text;
        var attribute = attributeModels.FirstOrDefault(a => _attributeNames.Contains(a.TypeDefinition.Name));

        if (attribute != null) {
            functionName = attribute.Arguments.FirstOrDefault()?.Value?.ToString() ?? functionName;
        }

        return new RequestHandlerNameModel( functionName, "POST");
    }

    protected override RequestParameterInformation? GetParameterInfoFromAttributes(GeneratorSyntaxContext generatorSyntaxContext, MethodDeclarationSyntax methodDeclarationSyntax,
        RequestHandlerNameModel requestHandlerNameModel,
        ParameterSyntax parameter, IReadOnlyList<AttributeModel> attributeModels, int parameterIndex) {
        return null;
    }

    protected override IEnumerable<string> AttributeNames() => _attributeNames;
}