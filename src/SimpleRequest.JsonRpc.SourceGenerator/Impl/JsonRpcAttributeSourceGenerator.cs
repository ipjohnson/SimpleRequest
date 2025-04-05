using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.JsonRpc.SourceGenerator.Impl;

public class JsonRpcAttributeSourceGenerator : IDependencyModuleSourceGenerator {

    public void SetupGenerator(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> incrementalValueProvider) {
        var methodSelector = new SyntaxSelector<MethodDeclarationSyntax>(
            JsonRpcKnownTypes.JsonRpcServiceAttribute
        );

        var modelGenerator = new JsonRpcModelGenerator();

        var jsonEntryPoints = 
            incrementalValueProvider.Where(SelectJsonRpcEntryPoints);

        var requestModels = context.SyntaxProvider.CreateSyntaxProvider(
            methodSelector.Where,
            modelGenerator.GenerateRequestModel
        ).WithComparer(new RequestHandlerModelComparer());
        
        context.RegisterSourceOutput(
            jsonEntryPoints.Combine(requestModels.Collect()),
            new JsonRpcModuleGenerator().GenerateSource
            );
        
        context.RegisterSourceOutput(
            requestModels.Combine(jsonEntryPoints.Collect()),
            new JsonRpcRequestHandlerGenerator().GenerateSource
            );
    }

    private bool SelectJsonRpcEntryPoints((ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right) data) {
        return data.Left.AttributeModels.Any(
            a => a.TypeDefinition.Equals(JsonRpcKnownTypes.JsonRpcFunctionAttribute));
    }
}