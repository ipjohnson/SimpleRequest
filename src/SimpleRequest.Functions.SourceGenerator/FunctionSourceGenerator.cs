using DependencyModules.SourceGenerator.Impl;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl;
using ISourceGenerator = DependencyModules.SourceGenerator.Impl.ISourceGenerator;

namespace SimpleRequest.Functions.SourceGenerator;

[Generator]
public class FunctionSourceGenerator : BaseSourceGenerator {

    protected override IEnumerable<ISourceGenerator> AttributeSourceGenerators() {
        yield return new FunctionAttributeSourceGenerator();
        yield return new FilterAttributeSourceGenerator(
            KnownFunctionTypes.SimpleRequestFunctionsModuleAttribute,
            "FunctionsFilters"
        );
    }
}