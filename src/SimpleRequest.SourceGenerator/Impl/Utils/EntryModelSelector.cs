using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;

namespace SimpleRequest.SourceGenerator.Impl.Utils;

public static class EntryModelSelector {

    public static string GetEntryPointBasePath(this ModuleEntryPointModel model) {
        var basePathAttribute = model.AttributeModels.FirstOrDefault(a => a.TypeDefinition.Name == "BasePathAttribute");

        if (basePathAttribute != null) {
            return basePathAttribute.Arguments.FirstOrDefault()?.Value?.ToString() ?? "";
        }
        
        return "";
    }

    public static ModuleEntryPointModel GetModel(this IReadOnlyList<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> entryPoints) {
        foreach (var model in entryPoints) {
            if (model.Left.AttributeModels.Any(
                    a => a.ImplementedInterfaces.Any(
                        i => i.Equals(KnownRequestTypes.ISimpleRequestEntryAttribute)))) {
                return AdjustModuleNamespace(model);
            }
        }

        foreach (var model in entryPoints) {
            if (model.Left.ModuleFeatures.HasFlag(ModuleEntryPointFeatures.AutoGenerateModule)) {
                return AdjustModuleNamespace(model);
            }
        }
        
        return entryPoints.First().Left;
    }

    private static ModuleEntryPointModel AdjustModuleNamespace((ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right) model) {
        var entryPoint = model.Left;
                
        if (string.IsNullOrEmpty(entryPoint.EntryPointType.Namespace)) {
            return entryPoint with {
                EntryPointType = TypeDefinition.Get(
                    model.Right.RootNamespace, entryPoint.EntryPointType.Name)
            };
        }
                
        return entryPoint;
    }
}