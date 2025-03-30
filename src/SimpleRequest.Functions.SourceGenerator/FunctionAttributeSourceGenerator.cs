using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Utils;
using SimpleRequest.SourceGenerator.Impl.Writers;

namespace SimpleRequest.Functions.SourceGenerator;

public class FunctionAttributeSourceGenerator : BaseRequestAttributeSourceGenerator {
    private readonly IEqualityComparer<RequestHandlerModel> _comparer = new RequestHandlerModelComparer();
    private readonly FunctionRequestModelGenerator _modelGenerator = new ();
    private readonly SimpleRequestHandlerWriter _simpleRequestWriter = new ();
    private readonly SimpleRequestRoutingWriter _routingWriter = 
        new (KnownFunctionTypes.SimpleRequestFunctionsModuleAttribute, "FunctionRouting");
    
    protected override IEnumerable<ITypeDefinition> AttributeTypes() {
        yield return KnownFunctionTypes.FunctionAttribute;
    }

    protected override void GenerateRouteFile(SourceProductionContext context,
        ((ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Left, ImmutableArray<RequestHandlerModel> Right) tuple) {
        if (tuple.Left == null) {
            return;
        }

        var configuration = tuple.Left.Value.configurationModel;
        var entryPoint = tuple.Left.Value.model;

        _routingWriter.WriteRouteFile(context, entryPoint, configuration, tuple.Right);
    }
    
    protected override void GenerateRequestFile(SourceProductionContext context,
        (RequestHandlerModel Left, (ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Right) valueTuple) {
        if (valueTuple.Right == null) {
            return;
        }
        
        _simpleRequestWriter.WriteRequestFile(context, valueTuple.Left, valueTuple.Right.Value);
    }

    protected override IEqualityComparer<RequestHandlerModel> GetComparer() {
        return _comparer;
    }

    protected override RequestHandlerModel GenerateAttributeModel(GeneratorSyntaxContext context, CancellationToken token) {
        return _modelGenerator.GenerateRequestModel(context, token);
    }
}