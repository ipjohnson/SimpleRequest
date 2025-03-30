using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SimpleRequest.RazorBlade.SourceGenerator.Impl;

public class RazorFileSourceGenerator : IDependencyModuleSourceGenerator {

    public void SetupGenerator(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> incrementalValueProvider) {

        var modelConverter = new CshtmlFileModelConverter();
        var entryPoint = new EntryPointGenerator();
        var templateGenerator = new CsharpTemplateGenerator();

        var combinedProvider =
            context.AdditionalTextsProvider.Combine(context.AnalyzerConfigOptionsProvider)
                .Where(FileFilter).Select(modelConverter.ToTemplateModel).Where(a => a is not null);

        context.RegisterSourceOutput(
            combinedProvider.Combine(incrementalValueProvider.Collect()),
            templateGenerator.GenerateSource
        );
        
        context.RegisterSourceOutput(
            incrementalValueProvider.Collect().Combine(combinedProvider.Collect()),
            entryPoint.GenerateTemplateEntry
        );
    }

    private bool FileFilter((AdditionalText Left, AnalyzerConfigOptionsProvider Right) filterData) {
        return filterData.Left.Path.EndsWith(".cshtml");
    }
}