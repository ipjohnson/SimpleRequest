using DependencyModules.SourceGenerator.Impl;
using Microsoft.CodeAnalysis;
using SimpleRequest.JsonRpc.SourceGenerator.Impl;
using SimpleRequest.SourceGenerator.Impl;

namespace SimpleRequest.JsonRpc.SourceGenerator;

[Generator]
public class JsonRpcSourceGenerator : BaseSourceGenerator {
    
    
    protected override IEnumerable<IDependencyModuleSourceGenerator> AttributeSourceGenerators() {
        yield return new JsonRpcAttributeSourceGenerator();
        yield return new FilterAttributeSourceGenerator(
            "JsonRpc"
        );
    }
}