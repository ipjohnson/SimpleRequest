using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl;
using ISourceGenerator = DependencyModules.SourceGenerator.Impl.ISourceGenerator;

namespace SimpleRequest.Web.SourceGenerator;

[Generator]
public class WebSourceGenerator : BaseSourceGenerator {

    protected override IEnumerable<ISourceGenerator> AttributeSourceGenerators() {
        
        yield return new WebAttributeSourceGenerator();
        yield return new FilterAttributeSourceGenerator(
            KnownWebTypes.SimpleRequestWebModuleAttribute,
            "WebFilters"
        );
    }
}