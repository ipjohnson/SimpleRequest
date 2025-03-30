using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Handler;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.Functions.SourceGenerator;

public class FunctionRequestModelGenerator : BaseRequestModelGenerator {
    private string _attributeName = KnownFunctionTypes.FunctionAttribute.Name;

    protected override RequestHandlerNameModel GetRequestNameModel(GeneratorSyntaxContext context,
        MethodDeclarationSyntax methodDeclaration,
        IReadOnlyList<AttributeModel> attributes,
        IReadOnlyList<AttributeModel> classAttributes,
        CancellationToken cancellation) {

        var functionName = methodDeclaration.Identifier.Text;
        foreach (var attributeModel in attributes) {
            if (attributeModel.TypeDefinition.Name == _attributeName) {
                var property =
                    attributeModel.Properties.FirstOrDefault(p => p.Name == "Name");

                functionName = property?.Value?.ToString() ?? functionName;
            }
        }

        return new RequestHandlerNameModel(
            functionName, "POST");
    }


    protected override RequestParameterInformation? GetParameterInfoFromAttributes(GeneratorSyntaxContext generatorSyntaxContext,
        MethodDeclarationSyntax methodDeclarationSyntax,
        RequestHandlerNameModel requestHandlerNameModel,
        ParameterSyntax parameter,
        IReadOnlyList<AttributeModel> attributeModels,
        int parameterIndex) {

        var models =
            AttributeModelHelper.GetAttributeModels(generatorSyntaxContext, parameter, CancellationToken.None);

        foreach (var attributeModel in models) {
            if (attributeModel != null) {
                switch (attributeModel.TypeDefinition.Name) {
                    default:
                        var result = DefaultGetParameterFromAttribute(
                            attributeModel, generatorSyntaxContext, parameter, parameterIndex, models);

                        if (result != null) {
                            return result;
                        }
                        break;
                }
            }
        }

        return null;
    }

    protected override IEnumerable<string> AttributeNames() {
        yield return "Function";
        yield return "FunctionAttribute";
    }

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