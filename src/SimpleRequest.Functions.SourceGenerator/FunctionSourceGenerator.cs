using DependencyModules.SourceGenerator.Impl;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl;

namespace SimpleRequest.Functions.SourceGenerator;

[Generator]
public class FunctionSourceGenerator : BaseSourceGenerator {

    protected override IEnumerable<IDependencyModuleSourceGenerator> AttributeSourceGenerators() {
        yield return new FunctionAttributeSourceGenerator();
        yield return new FilterAttributeSourceGenerator(
            KnownFunctionTypes.SimpleRequestFunctionsModuleAttribute,
            "FunctionsFilters"
        );
    }
}