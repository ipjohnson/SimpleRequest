using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Handler;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.Functions.SourceGenerator;

public class FunctionRequestModelGenerator : BaseRequestModelGenerator {

    protected override RequestHandlerNameModel GetRequestNameModel(
        GeneratorSyntaxContext context, 
        MethodDeclarationSyntax methodDeclaration,
        CancellationToken cancellation) {
        var attribute =
            methodDeclaration.GetAttribute(
                KnownFunctionTypes.FunctionAttribute.Name.Replace("Attribute",""))!;
        
        var argument = attribute.ArgumentList?.Arguments.FirstOrDefault();

        var functionName = argument?.Expression.ToString().Trim('"') ?? 
                           methodDeclaration.Identifier.Text;
        
        return new RequestHandlerNameModel(functionName, "POST");
    }

    protected override RequestParameterInformation? GetParameterInfoFromAttributes(
        GeneratorSyntaxContext generatorSyntaxContext, MethodDeclarationSyntax methodDeclarationSyntax, RequestHandlerNameModel requestHandlerNameModel,
        ParameterSyntax parameter, int parameterIndex) {
            foreach (var attributeList in parameter.AttributeLists) {
                foreach (var attribute in attributeList.Attributes) {
                    var attributeName = attribute.Name.ToString().Replace("Attribute", "");

                    switch (attributeName) {
                        case "FromContext":
                            var headerName =
                                attribute.ArgumentList?.Arguments.FirstOrDefault()?.ToFullString() ?? "";

                            return GetParameterInfoWithBinding(generatorSyntaxContext, parameter,
                                ParameterBindType.Header, headerName, parameterIndex);


                        default:
                            return DefaultGetParameterFromAttribute(
                                attribute, generatorSyntaxContext, parameter, parameterIndex);
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