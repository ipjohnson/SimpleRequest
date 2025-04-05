using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Handler;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.JsonRpc.SourceGenerator.Impl;

public class JsonRpcModelGenerator : BaseRequestModelGenerator {

    protected override RequestHandlerNameModel GetRequestNameModel(GeneratorSyntaxContext context, MethodDeclarationSyntax methodDeclaration, IReadOnlyList<AttributeModel> attributeModules, IReadOnlyList<AttributeModel> classAttributes,
        CancellationToken cancellation) {
        throw new NotImplementedException();
    }

    protected override RequestParameterInformation? GetParameterInfoFromAttributes(GeneratorSyntaxContext generatorSyntaxContext, MethodDeclarationSyntax methodDeclarationSyntax, RequestHandlerNameModel requestHandlerNameModel,
        ParameterSyntax parameter, IReadOnlyList<AttributeModel> attributeModels, int parameterIndex) {
        throw new NotImplementedException();
    }

    protected override IEnumerable<string> AttributeNames() {
        throw new NotImplementedException();
    }
}