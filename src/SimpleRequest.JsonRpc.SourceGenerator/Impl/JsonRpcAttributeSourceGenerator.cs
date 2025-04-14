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
            KnownTypesJsonRpc.JsonRpcFunctionAttribute
        );

        var modelGenerator = new JsonRpcModelGenerator();

        var jsonEntryPoints =
            incrementalValueProvider.Where(SelectJsonRpcEntryPoints);

        var requestModels = context.SyntaxProvider.CreateSyntaxProvider(
            methodSelector.Where,
            modelGenerator.GenerateRequestModel
        ).WithComparer(new RequestHandlerModelComparer());

        var moduleGenerator = new JsonRpcModuleGenerator();

        context.RegisterSourceOutput(
            requestModels.Combine(jsonEntryPoints.Collect()),
            new JsonRpcRequestHandlerGenerator().GenerateHandlerSource
        );
        
        context.RegisterSourceOutput(
            jsonEntryPoints.Collect().Combine(requestModels.Collect()),
            moduleGenerator.GenerateSharedDependencyModule
            );
        
        context.RegisterSourceOutput(
            jsonEntryPoints,
            moduleGenerator.GenerateRoutingSource
        );
    }

    private bool SelectJsonRpcEntryPoints((ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right) data) {
        return data.Left.AttributeModels.Any(
            a => a.TypeDefinition.Equals(KnownTypesJsonRpc.JsonRpcServiceAttribute));
    }
}