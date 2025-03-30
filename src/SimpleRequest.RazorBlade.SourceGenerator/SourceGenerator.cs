using DependencyModules.SourceGenerator.Impl;
using Microsoft.CodeAnalysis;
using SimpleRequest.RazorBlade.SourceGenerator.Impl;

namespace SimpleRequest.RazorBlade.SourceGenerator;

[Generator]
public class SourceGenerator : BaseSourceGenerator {

    protected override IEnumerable<IDependencyModuleSourceGenerator> AttributeSourceGenerators() {
        yield return new RazorFileSourceGenerator();
    }
} 