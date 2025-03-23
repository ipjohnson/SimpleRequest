using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;

namespace SimpleRequest.RazorBlade.SourceGenerator.Utils;

public class EntryPointSelector {
    public static ModuleEntryPointModel GetModel(IReadOnlyList<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> entryPoints) {
        foreach (var model in entryPoints) {
            if (model.Left.AttributeModels.Any(
                    a => a.ImplementedInterfaces.Any(
                        i => i.Equals(ISimpleRequestEntryAttribute)))) {
                var modelValue = model.Left;
                return modelValue with {
                    EntryPointType = TypeDefinition.Get(model.Right.RootNamespace, modelValue.EntryPointType.Name)
                };
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
    
    public static readonly ITypeDefinition ISimpleRequestEntryAttribute =
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            "SimpleRequest.Runtime.Attributes", 
            "ISimpleRequestEntryAttribute");

}