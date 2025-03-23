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
        (ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Left, ImmutableArray<RequestHandlerModel> Right) valueTuple) {
        if (valueTuple.Left.Length == 0) {
            return;
        }

        var configuration = valueTuple.Left.First().Right;
        var entryPoint = valueTuple.Left.GetModel();

        _routingWriter.WriteRouteFile(context, entryPoint,configuration, valueTuple.Right);
    }
    
    protected override void GenerateRequestFile(SourceProductionContext context,
        (RequestHandlerModel Left, ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Right) modelData) {
        _simpleRequestWriter.WriteRequestFile(context, modelData.Left, modelData.Right);
    }

    protected override IEqualityComparer<RequestHandlerModel> GetComparer() {
        return _comparer;
    }

    protected override RequestHandlerModel GenerateAttributeModel(GeneratorSyntaxContext context, CancellationToken token) {
        return _modelGenerator.GenerateRequestModel(context, token);
    }
}