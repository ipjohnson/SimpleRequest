using DependencyModules.SourceGenerator.Impl;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl;

namespace SimpleRequest.SourceGenerator;

[Generator]
public class RequestSourceGenerator : BaseSourceGenerator {

    protected override IEnumerable<IDependencyModuleSourceGenerator> AttributeSourceGenerators() {
        yield return new RequestAttributeSourceGenerator();
        yield return new FilterAttributeSourceGenerator(
            "RequestFilters"
        );
        yield return new OperationsHandlerAttributeSourceGenerator();
    }
}