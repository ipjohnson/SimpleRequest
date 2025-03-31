using DependencyModules.SourceGenerator.Impl;
using Microsoft.CodeAnalysis;

namespace SimpleRequest.JsonRpc.SourceGenerator;

[Generator]
public class JsonRpcSourceGenerator : BaseSourceGenerator {
    protected override IEnumerable<IDependencyModuleSourceGenerator> AttributeSourceGenerators() {
        yield break;
    }
}