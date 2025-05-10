using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Utils;

namespace SimpleRequest.SourceGenerator.Impl;

public abstract class BaseRequestAttributeSourceGenerator : IDependencyModuleSourceGenerator {

    protected abstract IEnumerable<ITypeDefinition> AttributeTypes();

    protected abstract void GenerateRouteFile(SourceProductionContext context,
        ((ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Left, ImmutableArray<RequestHandlerModel> Right) tuple);
    
    protected abstract void GenerateRequestFile(SourceProductionContext context, 
        (RequestHandlerModel Left, (ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Right) valueTuple);

    protected abstract IEqualityComparer<RequestHandlerModel> GetComparer();

    protected abstract RequestHandlerModel GenerateAttributeModel(GeneratorSyntaxContext context, CancellationToken token);

    public void SetupGenerator(IncrementalGeneratorInitializationContext context, 
        IncrementalValuesProvider<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> incrementalValueProvider) {
        var methodSelector = new ClassMethodSelector(AttributeTypes().ToArray());

        var selectedProvider = incrementalValueProvider.Collect().Select(SelectOneEntryPoint);
        
        var requestModelProvider = context.SyntaxProvider.CreateSyntaxProvider(
            methodSelector.Where,
            GenerateAttributeModel
        ).WithComparer(GetComparer());

        var collection =
            requestModelProvider.Collect();
        
        context.RegisterSourceOutput(
            selectedProvider.Combine(collection).Select(AdjustBasePath),
            GenerateRouteFile
        );

        context.RegisterSourceOutput(
            requestModelProvider.Combine(selectedProvider).Select(AdjustBasePath),
            GenerateRequestFile
        );
    }

    private (RequestHandlerModel Left, (ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Right) 
        AdjustBasePath((RequestHandlerModel Left, (ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Right) data, CancellationToken cancellationToken) {

        if (data.Right == null) {
            return data;
        }
        
        var basePath = 
            data.Right.Value.model.AttributeModels.FirstOrDefault(
                a => a.TypeDefinition.Name == "BasePathAttribute")?.Arguments.FirstOrDefault()?.Value as string;

        basePath = basePath?.Trim('"');

        if (string.IsNullOrEmpty(basePath)) {
            return data;
        }
        
        var model = AdjustModelBasePath(data.Left, basePath!);
        
        return (model, data.Right);
    }
    
    private ((ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Left, ImmutableArray<RequestHandlerModel> Right) 
        AdjustBasePath(((ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Left, ImmutableArray<RequestHandlerModel> Right) data, CancellationToken cancellationToken) {
        
        if (data.Left == null) {
            return data;
        }
        
        var basePath = 
            data.Left.Value.model.AttributeModels.FirstOrDefault(
                a => a.TypeDefinition.Name == "BasePathAttribute")?.Arguments.FirstOrDefault()?.Value as string;

        basePath = basePath?.Trim('"');

        if (string.IsNullOrEmpty(basePath)) {
            return data;
        }
        
        var newArray = 
            data.Right.Select(a => AdjustModelBasePath(a, basePath!)).ToImmutableArray();
        
        return (data.Left, newArray);
    }
    
    private RequestHandlerModel AdjustModelBasePath(RequestHandlerModel dataLeft, string basePath) {
        return dataLeft with {
            Name = dataLeft.Name with {
                Path = CombinePath(basePath, dataLeft.Name.Path)
            }
        };
    }

    private string CombinePath(string basePath, string namePath) {
        if (namePath.StartsWith("/")) {
            namePath = namePath.TrimStart('/');
        }

        if (basePath.EndsWith("/")) {
            basePath = basePath.TrimEnd('/');
        }
        
        return basePath + "/" + namePath;
    }

    private (ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)?
        SelectOneEntryPoint(ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> data, CancellationToken context) {
        if (data.Length == 0) {
            return null;
        }
        
        var entryPoint = data.GetModel();
        var configuration = data.First().Right;
        
        return (entryPoint, configuration);
    }
}