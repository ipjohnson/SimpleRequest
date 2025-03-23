using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ISourceGenerator = DependencyModules.SourceGenerator.Impl.ISourceGenerator;

namespace SimpleRequest.RazorBlade.SourceGenerator.Impl;

public class RazorFileSourceGenerator : ISourceGenerator {

    public void SetupGenerator(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> incrementalValueProvider) {

        var modelConverter = new CshtmlFileModelConverter();
        var entryPoint = new EntryPointGenerator();
        var templateGenerator = new CsharpTemplateGenerator();
        
        var provider =
            context.AdditionalTextsProvider;

        var combinedProvider =
            provider.Combine(context.AnalyzerConfigOptionsProvider).Where(FileFilter).Select(modelConverter.ToTemplateModel).Where(a => a is not null);

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