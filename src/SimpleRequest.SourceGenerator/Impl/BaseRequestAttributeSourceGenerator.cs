using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Models;
using ISourceGenerator = DependencyModules.SourceGenerator.Impl.ISourceGenerator;

namespace SimpleRequest.SourceGenerator.Impl;

public abstract class BaseRequestAttributeSourceGenerator : ISourceGenerator {

    protected abstract IEnumerable<ITypeDefinition> AttributeTypes();

    protected abstract void GenerateRouteFile(SourceProductionContext context,
        ((ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right) Left, ImmutableArray<RequestHandlerModel> Right) modelData);


    protected abstract void GenerateRequestFile(SourceProductionContext context, 
        (RequestHandlerModel Left, ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Right) modelData);

    protected abstract IEqualityComparer<RequestHandlerModel> GetComparer();

    protected abstract RequestHandlerModel GenerateAttributeModel(GeneratorSyntaxContext context, CancellationToken token);

    public void SetupGenerator(IncrementalGeneratorInitializationContext context, 
        IncrementalValuesProvider<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> incrementalValueProvider) {
        var methodSelector = new SyntaxSelector<MethodDeclarationSyntax>(AttributeTypes().ToArray());
        
        var requestModelProvider = context.SyntaxProvider.CreateSyntaxProvider(
            methodSelector.Where,
            GenerateAttributeModel
        ).WithComparer(GetComparer());

        var collection =
            requestModelProvider.Collect();

        context.RegisterSourceOutput(
            incrementalValueProvider.Combine(collection),
            GenerateRouteFile
        );

        var modelEntryCollection = 
            incrementalValueProvider.Collect();

        context.RegisterSourceOutput(
            requestModelProvider.Combine(modelEntryCollection),
            GenerateRequestFile
        );
    }
}