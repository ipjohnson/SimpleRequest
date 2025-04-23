using System.Collections.Immutable;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Utils;
using SimpleRequest.SourceGenerator.Impl.Writers;

namespace SimpleRequest.SourceGenerator.Impl;

public class OperationsHandlerAttributeSourceGenerator : IDependencyModuleSourceGenerator {
    private readonly IEqualityComparer<RequestHandlerModel> _comparer = new RequestHandlerModelComparer();

    private readonly SimpleRequestHandlerWriter _simpleRequestWriter = new ();
    private readonly SimpleRequestRoutingWriter _routingWriter = 
        new ("OperationHandlerRouting", "StandardHandler", "Handler");
    
    public void SetupGenerator(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> incrementalValueProvider) {
        var classSelect = new SyntaxSelector<ClassDeclarationSyntax>(KnownRequestTypes.Attributes.OperationsHandler);

        var selectedProvider = incrementalValueProvider.Collect().Select(SelectOneEntryPoint);

        var classRequestModelGenerator = new RequestHandlerCollectionModelGenerator();

        var requestModelProvider = context.SyntaxProvider.CreateSyntaxProvider(
            classSelect.Where,
            classRequestModelGenerator.GenerateModel
        ).SelectMany((collection,token) => {
            token.ThrowIfCancellationRequested();
            
            return collection.RequestHandlers;
        }).WithComparer(_comparer);
        
        context.RegisterSourceOutput(
            requestModelProvider.Combine(selectedProvider).Select(AdjustBasePath),
            GenerateRequestFile
        );
        
        var collection =
            requestModelProvider.Collect();
        
        context.RegisterSourceOutput(
            selectedProvider.Combine(collection).Select(AdjustBasePath),
            GenerateRouteFile
        );
    }
    
    protected void GenerateRouteFile(SourceProductionContext context,
        ((ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Left, ImmutableArray<RequestHandlerModel> Right) tuple) {
        if (tuple.Left == null) {
            return;
        }

        var configuration = tuple.Left.Value.configurationModel;
        var entryPoint = tuple.Left.Value.model;

        _routingWriter.WriteRouteFile(context, entryPoint, configuration, tuple.Right);
    }
    
    protected void GenerateRequestFile(SourceProductionContext context,
        (RequestHandlerModel Left, (ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Right) valueTuple) {
        if (valueTuple.Right == null) {
            return;
        }
        
        _simpleRequestWriter.WriteRequestFile(context, valueTuple.Left, valueTuple.Right.Value);
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