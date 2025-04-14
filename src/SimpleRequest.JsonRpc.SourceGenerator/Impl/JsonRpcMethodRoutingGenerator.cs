using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Routing;

namespace SimpleRequest.JsonRpc.SourceGenerator.Impl;

public static class JsonRpcMethodRoutingGenerator {

    public static void Generate(SourceProductionContext context,
        (ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Left, ImmutableArray<RequestHandlerModel> Right) data,
        DependencyModuleConfigurationModel configurationModel,
        ModuleEntryPointModel sharedEntryPoint) {

        var csharpFile = new CSharpFileDefinition(configurationModel.RootNamespace + ".Generated");

        var classDefinition = csharpFile.AddClass("JsonRpcSharedModule");

        classDefinition.Modifiers = ComponentModifier.Public | ComponentModifier.Partial;

        GenerateMethodRoutingClasses(context, classDefinition, data, configurationModel, sharedEntryPoint);

        WriteCSharpFile(context, csharpFile, data, configurationModel, sharedEntryPoint);
    }

    private static void GenerateMethodRoutingClasses(SourceProductionContext context, ClassDefinition classDefinition,
        (ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Left, ImmutableArray<RequestHandlerModel> Right) data,
        DependencyModuleConfigurationModel configurationModel,
        ModuleEntryPointModel sharedEntryPoint) {
        
        var methodDictionary = GetMethodsByTag(data.Right);

        int count = 0;
        foreach (var kvp in methodDictionary) {
            context.CancellationToken.ThrowIfCancellationRequested();
            
            GenerateRoutingClassForTag(context, classDefinition, kvp.Key, kvp.Value, configurationModel, sharedEntryPoint, count++);
        }
    }

    private static void GenerateRoutingClassForTag(SourceProductionContext context,
        ClassDefinition classDefinition, string tag, Dictionary<string, RequestHandlerModel> kvpValue,
        DependencyModuleConfigurationModel configurationModel, ModuleEntryPointModel sharedEntryPoint, int count) {
        var routingClassName = $"MethodRouting{count}";
        
        var routingClassWriter = new RoutingClassGenerator(routingClassName, "JsonRpc");
        
        routingClassWriter.GenerateRoutingClass(
            context, sharedEntryPoint, kvpValue.Values.ToList(), classDefinition);

        CreateRegistrationField(sharedEntryPoint, classDefinition, routingClassName, tag);
    }

    private static void CreateRegistrationField(ModuleEntryPointModel sharedEntryPoint, ClassDefinition classDefinition, string routingClassName, string tag) {
        var registryType = new GenericTypeDefinition(
            TypeDefinitionEnum.ClassDefinition,
            KnownTypes.DependencyModules.Helpers.Namespace,
            "DependencyRegistry",
            new[] {
                sharedEntryPoint.EntryPointType
            });

        var routingField = classDefinition.AddField(typeof(int), "_init" + routingClassName);
        
        routingField.Modifiers |= ComponentModifier.Private | ComponentModifier.Static;
        routingField.InitializeValue = new StaticInvokeGenericStatement(
            registryType,
            "Add",
            new [] {
                KnownRequestTypes.IRequestHandlerProvider
            },
            new[] {
                SyntaxHelpers.TypeOf(TypeDefinition.Get(
                    sharedEntryPoint.EntryPointType.Namespace,
                    "JsonRpcSharedModule." + routingClassName)),
                CodeOutputComponent.Get("ServiceLifetime.Transient"),
                CodeOutputComponent.Get(SyntaxHelpers.QuoteString("json-rpc:" + tag))
            }
        ) {
            Indented = false
        };
        
        routingField.AddUsingNamespace("Microsoft.Extensions.DependencyInjection");
    }

    private static Dictionary<string, Dictionary<string,RequestHandlerModel>> GetMethodsByTag(ImmutableArray<RequestHandlerModel> handlers) {
        var methods = new Dictionary<string, Dictionary<string,RequestHandlerModel>>();

        foreach (var handlerModel in handlers) {
            var attribute =
                handlerModel.Filters.FirstOrDefault(
                    a => a.TypeDefinition.Equals(KnownTypesJsonRpc.JsonRpcFunctionAttribute));

            if (attribute == null) {
                continue;
            }

            if (attribute.Properties.FirstOrDefault(
                    a => a.Name.Equals("Tag"))?.Value is not string[] tag || tag.Length == 0) {
                tag = new[] {
                    JsonRpcConstants.DefaultTag
                };
            }

            foreach (var tagItem in tag) {
                foreach (var tagString in tagItem.Split(JsonRpcConstants.TagDelimiter)) {
                    if (!methods.ContainsKey(tagString)) {
                        methods.Add(tagString, new Dictionary<string,RequestHandlerModel>());
                    }

                    methods[tagString][handlerModel.Name.Path] = handlerModel;
                }
            }
        }

        return methods;
    }

    private static void WriteCSharpFile(SourceProductionContext context,
        CSharpFileDefinition csharpFile,
        (ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Left, ImmutableArray<RequestHandlerModel> Right) data, DependencyModuleConfigurationModel configurationModel,
        ModuleEntryPointModel sharedEntryPoint) {

        var outputContext = new OutputContext(new OutputContextOptions {
            TypeOutputMode = TypeOutputMode.Global
        });

        csharpFile.WriteOutput(outputContext);

        context.AddSource("Generated.JsonRpcSharedModule.JsonMethodRouting.g.cs", outputContext.Output());
    }
}