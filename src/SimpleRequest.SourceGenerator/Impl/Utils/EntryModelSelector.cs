using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;

namespace SimpleRequest.SourceGenerator.Impl.Utils;

public static class EntryModelSelector {

    public static ModuleEntryPointModel GetModel(this IReadOnlyList<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> entryPoints) {
        foreach (var model in entryPoints) {
            if (model.Left.AttributeModels.Any(
                    a => a.ImplementedInterfaces.Any(
                        i => i.Equals(KnownRequestTypes.ISimpleRequestEntryAttribute)))) {
                return model.Left;
            }
        }

        foreach (var model in entryPoints) {
            if (model.Left.ModuleFeatures.HasFlag(ModuleEntryPointFeatures.AutoGenerateModule)) {
                var modelValue = model.Left;
                return modelValue with {
                    EntryPointType = TypeDefinition.Get(model.Right.RootNamespace, modelValue.EntryPointType.Name)
                };
            }
        }
        
        return entryPoints.First().Left;
    }
}