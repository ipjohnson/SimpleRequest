using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using SimpleRequest.RazorBlade.SourceGenerator.Models;
using SimpleRequest.RazorBlade.SourceGenerator.Utils;
using static CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.RazorBlade.SourceGenerator.Impl;

public class CsharpTemplateGenerator {

    public void GenerateSource(SourceProductionContext context,
        (CshtmlFileModel Left, ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Right) data) {
        if (data.Right.Length == 0) {
            return;
        }

        var entryPoint = EntryPointSelector.GetModel(data.Right);
        
        var namespaceString = NamespaceUtility.GetTemplateNamespace(entryPoint, data.Left.FilePath);
        var className = Path.GetFileNameWithoutExtension(data.Left.FilePath);

        context.CancellationToken.ThrowIfCancellationRequested();
        
        var file = GenerateCSharpFile(context, data.Left, entryPoint, namespaceString, className);

        var output = new OutputContext();
        
        file.WriteOutput(output);
        var type = TypeDefinition.Get(namespaceString, className);
        
        context.AddSource(
            type.GetFileNameHint(data.Right.First().Right.RootNamespace,"RazorTemplates"), output.Output());
    }

    private CSharpFileDefinition GenerateCSharpFile(
        SourceProductionContext context, 
        CshtmlFileModel dataLeft, 
        ModuleEntryPointModel entryPoint, 
        string namespaceString,
        string className) {
        var csharpFile = new CSharpFileDefinition(namespaceString);
        var classDefinition = csharpFile.AddClass(className);
        
        classDefinition.Modifiers = ComponentModifier.NoAccessibility | ComponentModifier.Partial;
        classDefinition.AddBaseType(ITemplateContextAware);
        classDefinition.AddBaseType(IRazorBladeTemplate);

        foreach (var ns in dataLeft.Namespaces) {
            classDefinition.AddUsingNamespace(ns);
        }
        
        GenerateCreateMethod(classDefinition, dataLeft);
        GenerateContextProperty(classDefinition);
        
        return csharpFile;
    }

    private void GenerateContextProperty(ClassDefinition classDefinition) {
        classDefinition.AddProperty(
            TypeDefinition.Get("","IRequestContext"),
            "RequestContext"
            );
    }

    private void GenerateCreateMethod(ClassDefinition classDefinition, CshtmlFileModel dataLeft) {
        var method = classDefinition.AddMethod("Create");
        var classDef = TypeDefinition.Get("", classDefinition.Name);

        method.Modifiers |= ComponentModifier.Public | ComponentModifier.Static;
        
        method.SetReturnType(classDef);
        var context = method.AddParameter(
            TypeDefinition.Get("SimpleRequest.Runtime.Invoke", "IRequestContext"), "context");

        if (string.IsNullOrEmpty(dataLeft.ModelName)) {
            method.Return(New(classDef));
            return;
        }
        
        var model = method.Assign(SyntaxHelpers.StaticCast(
            TypeDefinition.Get("", dataLeft.ModelName),
            context.Property("ResponseData").Property("ResponseValue"))).ToVar("model");

        method.If(EqualsStatement(model, Null())).Throw(typeof(Exception),QuoteString("Model response not found"));
        
        method.Return(New(classDef, model));
    }
    
   
    static readonly ITypeDefinition ITemplateContextAware = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            "SimpleRequest.Runtime.Templates", 
            "ITemplateContextAware");
    
    static readonly ITypeDefinition IRazorBladeTemplate = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            "SimpleRequest.RazorBlade.Impl", 
            "IRazorBladeTemplate");
}