using System.Collections.Immutable;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Utils;
using SimpleRequest.SourceGenerator.Impl.Writers;

namespace SimpleRequest.JsonRpc.SourceGenerator.Impl;

public class JsonRpcRequestHandlerGenerator {
    private readonly SimpleRequestHandlerWriter _simpleRequestWriter = new ();


    public void GenerateHandlerSource(SourceProductionContext context, 
        (RequestHandlerModel Left, ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Right) data) {
        if (data.Right.Length == 0) {
            return;
        }
        
        var entryModel = data.Right.GetModel();

        _simpleRequestWriter.WriteRequestFile(
            context, 
            data.Left, 
            new ValueTuple<ModuleEntryPointModel, DependencyModuleConfigurationModel>(entryModel, data.Right.First().Right));
    }
}