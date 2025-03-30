using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Utils;
using SimpleRequest.SourceGenerator.Impl.Writers;

namespace SimpleRequest.Web.SourceGenerator;

public class WebAttributeSourceGenerator : BaseRequestAttributeSourceGenerator {
    private readonly IEqualityComparer<RequestHandlerModel> _comparer = new RequestHandlerModelComparer();
    private readonly SimpleRequestHandlerWriter _simpleRequestWriter = new();
    private readonly WebRequestModelGenerator _webRequestModelGenerator = new();

    private readonly SimpleRequestRoutingWriter _routingWriter =
        new(KnownWebTypes.SimpleRequestWebModuleAttribute, "FunctionRouting");

    protected override IEnumerable<ITypeDefinition> AttributeTypes() {
        yield return KnownWebTypes.Attributes.Delete;
        yield return KnownWebTypes.Attributes.Get;
        yield return KnownWebTypes.Attributes.Patch;
        yield return KnownWebTypes.Attributes.Post;
        yield return KnownWebTypes.Attributes.Put;
    }

    protected override void GenerateRouteFile(SourceProductionContext context,
        ((ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Left, ImmutableArray<RequestHandlerModel> Right) tuple) {
        if (tuple.Left == null) {
            return;
        }

        var entryPoint = tuple.Left.Value.model;
        var configuration = tuple.Left.Value.configurationModel;
        var handlerModels = tuple.Right;

        _routingWriter.WriteRouteFile(context, entryPoint, configuration, handlerModels);
    }

    protected override void GenerateRequestFile(SourceProductionContext context,
        (RequestHandlerModel Left, (ModuleEntryPointModel model, DependencyModuleConfigurationModel configurationModel)? Right) valueTuple) {
        var model = valueTuple.Left;
        var entryPointInfo = valueTuple.Right;
        if (entryPointInfo == null) {
            return;
        }

        _simpleRequestWriter.WriteRequestFile(context, model, entryPointInfo.Value);
    }


    protected override IEqualityComparer<RequestHandlerModel> GetComparer() {
        return _comparer;
    }

    protected override RequestHandlerModel GenerateAttributeModel(GeneratorSyntaxContext context, CancellationToken token) {
        return _webRequestModelGenerator.GenerateRequestModel(context, token);
    }
}