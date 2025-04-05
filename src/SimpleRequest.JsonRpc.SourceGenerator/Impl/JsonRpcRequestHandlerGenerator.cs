using System.Collections.Immutable;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.JsonRpc.SourceGenerator.Impl;

public class JsonRpcRequestHandlerGenerator {

    public void GenerateSource(SourceProductionContext context, 
        (RequestHandlerModel Left, ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Right) data) {
        
    }
}