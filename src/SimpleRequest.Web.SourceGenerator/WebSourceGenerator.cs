using DependencyModules.SourceGenerator.Impl;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl;

namespace SimpleRequest.Web.SourceGenerator;

[Generator]
public class WebSourceGenerator : BaseSourceGenerator {

    protected override IEnumerable<IDependencyModuleSourceGenerator> AttributeSourceGenerators() {

        yield return new WebAttributeSourceGenerator();
        yield return new FilterAttributeSourceGenerator(
            KnownWebTypes.SimpleRequestWebModuleAttribute,
            "WebFilters"
        );
    }
}