using System.Collections.Immutable;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.JsonRpc.SourceGenerator.Impl;

public class JsonRpcModuleGenerator {

    public void GenerateSource(SourceProductionContext context,
        ((ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right) Left, ImmutableArray<RequestHandlerModel> Right) data) {
        
    }
}