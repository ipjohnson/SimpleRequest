using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.RazorBlade.SourceGenerator.Models;
using static CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.RazorBlade.SourceGenerator.Impl;

public class EntryPointGenerator {

    public void GenerateTemplateEntry(SourceProductionContext context,
        ((ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right) Left, ImmutableArray<CshtmlFileModel> Right) data) {
        context.CancellationToken.ThrowIfCancellationRequested();
        
        if (!data.Left.Left.AttributeModels.Any(
                a => a.ImplementedInterfaces.Any(i => i.Name == "ISimpleRequestEntryAttribute"))) {
            return;
        }

        var csharpFile = GenerateCSharpFile(data.Left.Left, data.Right, context.CancellationToken);

        var output = new OutputContext();

        csharpFile.WriteOutput(output);

        context.AddSource(
            $"{data.Left.Left.EntryPointType.Namespace}.{data.Left.Left.EntryPointType.Name}.TemplateProvider.g.cs",
            output.Output());
    }

    private CSharpFileDefinition GenerateCSharpFile(ModuleEntryPointModel entryPointModel, ImmutableArray<CshtmlFileModel> dataRight, CancellationToken contextCancellationToken) {
        var csharpFile = new CSharpFileDefinition(entryPointModel.EntryPointType.Namespace);

        var classDefinition = csharpFile.AddClass(entryPointModel.EntryPointType.Name);
        classDefinition.Modifiers |= ComponentModifier.Public | ComponentModifier.Partial;

        GenerateTemplateProviderClass(classDefinition, entryPointModel, dataRight, contextCancellationToken);

        var field = classDefinition.AddField(typeof(int), "_templateRegistration");

        field.Modifiers |= ComponentModifier.Private | ComponentModifier.Static;

        var closedType = new GenericTypeDefinition(
            TypeDefinitionEnum.ClassDefinition, KnownTypes.DependencyModules.Helpers.Namespace, "DependencyRegistry", new[] {
                entryPointModel.EntryPointType
            });

        var invokeStatement = new StaticInvokeGenericStatement(closedType, "Add", new[] {
            TypeDefinition.Get("SimpleRequest.Runtime.Templates", "ITemplateProvider"),
        }, new[] {
            TypeOf(TypeDefinition.Get("", "TemplateProvider")), CodeOutputComponent.Get("ServiceLifetime.Singleton"),
        }) {
            Indented = false
        };

        invokeStatement.AddUsingNamespace(KnownTypes.Microsoft.DependencyInjection.Namespace);

        field.InitializeValue = invokeStatement;

        return csharpFile;
    }

    private void GenerateTemplateProviderClass(ClassDefinition classDefinition, ModuleEntryPointModel entryPointModel, ImmutableArray<CshtmlFileModel> dataRight, CancellationToken contextCancellationToken) {
        var templateProvider = classDefinition.AddClass("TemplateProvider");

        templateProvider.AddBaseType(TypeDefinition.Get("SimpleRequest.Runtime.Templates", "ITemplateProvider"));
        var constructor = templateProvider.AddConstructor();
        var engineType = TypeDefinition.Get("SimpleRequest.RazorBlade.Impl", "IRazorBladeInvocationEngine");
        var parameter = constructor.AddParameter(
            engineType,
            "engine"
        );

        var engineField = templateProvider.AddField(engineType, "_engine");

        constructor.Assign(parameter).To(engineField.Instance);

        var method = templateProvider.AddMethod("GetTemplates");

        method.SetReturnType(TypeDefinition.IEnumerable(TypeDefinition.Get("", "TemplateInfo")));

        if (dataRight.Length > 0) {
            foreach (var model in dataRight) {
                contextCancellationToken.ThrowIfCancellationRequested();
                var namespaceString = NamespaceUtility.GetTemplateNamespace(entryPointModel, model.FilePath);
                var className = Path.GetFileNameWithoutExtension(model.FilePath);

                var newStatement = New(
                    TypeDefinition.Get("", "TemplateInfo"),
                    QuoteString(className),
                    engineField.Instance.Invoke("CreateInvocationDelegate", $"{namespaceString}.{className}.Create")
                );

                method.AddIndentedStatement(YieldReturn(newStatement));
            }
        }
        else {
            method.AddIndentedStatement("yield break");
        }
    }
}