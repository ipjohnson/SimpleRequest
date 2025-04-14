using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Routing;

namespace SimpleRequest.JsonRpc.SourceGenerator.Impl;

public class JsonRpcModuleGenerator {
    private const string JsonSharedClassName = "JsonRpcSharedModule";
    private readonly RoutingClassGenerator _routingClassGenerator = new("JsonRpcRouting", "JsonRpcHandler");

    public void GenerateRoutingSource(SourceProductionContext context, (ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right) data) {

        var tagDictionary = GenerateModelTagDictionary(data.Left, data.Right);

        context.CancellationToken.ThrowIfCancellationRequested();

        var csharpFile = new CSharpFileDefinition(data.Left.EntryPointType.Namespace);

        GenerateRoutingClass(context, data.Left, data.Right, csharpFile, tagDictionary);

        WriteFile(context, data.Left, data.Right, csharpFile);
    }

    private void GenerateRoutingClass(SourceProductionContext context, ModuleEntryPointModel dataLeft, DependencyModuleConfigurationModel configurationModel, CSharpFileDefinition csharpFile,
        Dictionary<string, IReadOnlyList<string>> tagDictionary) {
        var classDefinition = csharpFile.AddClass(dataLeft.EntryPointType.Name);

        SetupStaticRegistrations(dataLeft, configurationModel, classDefinition);

        classDefinition.Modifiers |= ComponentModifier.Partial | ComponentModifier.Public;

        var handlerInfoDictionary = GenerateHandlerInfoClasses(classDefinition, dataLeft, configurationModel, tagDictionary);

        _routingClassGenerator.GenerateRoutingClass(context, dataLeft, GenerateRequestHandlerModel(handlerInfoDictionary), classDefinition);
    }

    private static void SetupStaticRegistrations(ModuleEntryPointModel dataLeft, DependencyModuleConfigurationModel configurationModel, ClassDefinition classDefinition) {
        var registryType = new GenericTypeDefinition(
            TypeDefinitionEnum.ClassDefinition,
            KnownTypes.DependencyModules.Helpers.Namespace,
            "DependencyRegistry",
            new[] {
                dataLeft.EntryPointType
            });
        
        var sharedField = classDefinition.AddField(typeof(int), "_sharedJsonDepModule");
        sharedField.Modifiers |= ComponentModifier.Private | ComponentModifier.Static;
        sharedField.InitializeValue = new StaticInvokeStatement(
            registryType,
            "AddModule",
            new[] {
                SyntaxHelpers.New(
                    TypeDefinition.Get(configurationModel.RootNamespace + ".Generated", JsonSharedClassName)
                )
            }
        ) {
            Indented = false
        };
        
        var routingField = classDefinition.AddField(typeof(int), "_jsonrpcRoutingDep");
        
        routingField.Modifiers |= ComponentModifier.Private | ComponentModifier.Static;
        routingField.InitializeValue = new StaticInvokeGenericStatement(
            registryType,
            "Add",
            new [] {
                KnownRequestTypes.IRequestHandlerProvider
            },
            new[] {
                SyntaxHelpers.TypeOf(TypeDefinition.Get("", "JsonRpcRouting"))
            }
        ) {
            Indented = false
        };
    }

    private IReadOnlyList<RequestHandlerModel> GenerateRequestHandlerModel(Dictionary<string, (ITypeDefinition, IReadOnlyList<string>)> tagDictionary) {
        var handlerList = new List<RequestHandlerModel>();

        foreach (var kvp in tagDictionary) {
            handlerList.Add(new RequestHandlerModel(
                new RequestHandlerNameModel(kvp.Key, "POST"),
                kvp.Value.Item1,
                "JsonRpcHandler",
                null,
                kvp.Value.Item1,
                Array.Empty<RequestParameterInformation>(),
                new ResponseInformationModel(true, null, null, null, null),
                Array.Empty<AttributeModel>()
            ));
        }

        return handlerList;
    }

    private Dictionary<string, ValueTuple<ITypeDefinition, IReadOnlyList<string>>> GenerateHandlerInfoClasses(
        ClassDefinition classDefinition, ModuleEntryPointModel dataLeft, DependencyModuleConfigurationModel dataRight, Dictionary<string, IReadOnlyList<string>> tagDictionary) {
        var handlerInfoDictionary = new Dictionary<string, ValueTuple<ITypeDefinition, IReadOnlyList<string>>>();

        classDefinition.AddUsingNamespace("System.ComponentModel");

        var count = 1;
        foreach (var kvp in tagDictionary) {
            var name = "JsonHandlerInfo" + count++;
            var handlerClass = classDefinition.AddClass(name);

            handlerClass.Modifiers |= ComponentModifier.Partial | ComponentModifier.Internal | ComponentModifier.Static;
            handlerClass.AddLeadingTrait(CodeOutputComponent.Get("[Browsable(false)]", true));

            var property = handlerClass.AddProperty(KnownRequestTypes.IRequestHandlerInfo, "HandlerInfo");

            property.Modifiers |= ComponentModifier.Internal | ComponentModifier.Static;
            property.Set = null;
            property.Get.Add(SyntaxHelpers.New(
                KnownTypesJsonRpc.JsonRpcRoutingHandlerInfo,
                SyntaxHelpers.QuoteString(kvp.Key),
                false,
                kvp.Value
            ));
            property.Get.LambdaSyntax = true;

            handlerInfoDictionary[kvp.Key] = (TypeDefinition.Get("", name), kvp.Value);
        }

        return handlerInfoDictionary;
    }

    private void WriteFile(SourceProductionContext context, ModuleEntryPointModel entryPointModel, DependencyModuleConfigurationModel dependencyModuleConfiguration, CSharpFileDefinition csharpFile) {
        context.CancellationToken.ThrowIfCancellationRequested();

        var outputContext = new OutputContext(
            new OutputContextOptions {
                TypeOutputMode = TypeOutputMode.Global
            }
        );

        csharpFile.WriteOutput(outputContext);

        var output = outputContext.Output();
        context.AddSource(
            entryPointModel.EntryPointType.GetFileNameHint(
                dependencyModuleConfiguration.RootNamespace,
                "JsonRpcRouting"),
            output);
    }


    private Dictionary<string, IReadOnlyList<string>> GenerateModelTagDictionary(ModuleEntryPointModel entryPointModel, DependencyModuleConfigurationModel configurationModel) {
        var tagDictionary = new Dictionary<string, IReadOnlyList<string>>();

        foreach (var attributeModel in entryPointModel.AttributeModels.Where(
                     a => a.TypeDefinition.Equals(KnownTypesJsonRpc.JsonRpcServiceAttribute))) {
            var tags =
                attributeModel.Properties.FirstOrDefault(a => a.Name == "Tags");
            var path = attributeModel.Properties.FirstOrDefault(
                a => a.Name == "Path");

            var tagArray = new List<string>();
            if (tags?.Value is string[] tagValues) {
                foreach (var tagValue in tagValues) {
                    tagArray.AddRange(tagValue.Split(JsonRpcConstants.TagDelimiter).Select(t => t.Trim()));
                }
            }

            if (tagArray.Count == 0) {
                tagArray.Add(JsonRpcConstants.DefaultTag);
            }

            var pathValue = path?.Value?.ToString();

            if (string.IsNullOrEmpty(pathValue)) {
                pathValue = "/";
            }

            tagDictionary[pathValue!] = tagArray;
        }

        return tagDictionary;
    }


    public void GenerateSharedDependencyModule(
        SourceProductionContext context,
        (ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Left, ImmutableArray<RequestHandlerModel> Right) data) {
        if (data.Left.Length == 0 || data.Right.Length == 0) {
            return;
        }

        var configurationModel = data.Left.First().Right;
        var firstEntryPoint = data.Left.First().Left;

        var dependencyWriter = new DependencyFileWriter(
            new FileLogger(configurationModel, "json-rpc-dependencies"));

        var sharedEntryPoint = new ModuleEntryPointModel(
            ModuleEntryPointFeatures.ShouldImplementEquals,
            "",
            TypeDefinition.Get(configurationModel.RootNamespace + ".Generated", "JsonRpcSharedModule"),
            null,
            false,
            false,
            null,
            firstEntryPoint.GenerateFactories,
            Array.Empty<ParameterInfoModel>(),
            Array.Empty<PropertyInfoModel>(),
            Array.Empty<AttributeModel>(),
            Array.Empty<ITypeDefinition>(),
            Array.Empty<ITypeDefinition>()
        );

        var output = dependencyWriter.Write(
            sharedEntryPoint,
            configurationModel,
            GetServiceModels(context, data, configurationModel),
            "JsonRpcShared");

        context.AddSource($"Generated.{JsonSharedClassName}.Dependencies.g.cs", output);

        var moduleWriter = new DependencyModuleWriter(false);

        moduleWriter.GenerateSource(context, ImmutableArray.Create(
            new ValueTuple<ModuleEntryPointModel, DependencyModuleConfigurationModel>(sharedEntryPoint, configurationModel)));
        
        GenerateMethodRouting(context, data, configurationModel, sharedEntryPoint);
    }

    private void GenerateMethodRouting(SourceProductionContext context, (ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Left, ImmutableArray<RequestHandlerModel> Right) data, DependencyModuleConfigurationModel configurationModel, ModuleEntryPointModel sharedEntryPoint) {
        context.CancellationToken.ThrowIfCancellationRequested();
        
        JsonRpcMethodRoutingGenerator.Generate(context, data, configurationModel, sharedEntryPoint);
        
    }

    private IEnumerable<ServiceModel> GetServiceModels(SourceProductionContext context,
        (ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Left, ImmutableArray<RequestHandlerModel> Right) data,
        DependencyModuleConfigurationModel configurationModel) {
        foreach (var handlerModel in data.Right) {
            context.CancellationToken.ThrowIfCancellationRequested();

            yield return new ServiceModel(
                handlerModel.HandlerType,
                handlerModel.ConstructorInfo,
                null,
                null,
                new[] {
                    new ServiceRegistrationModel(
                        handlerModel.HandlerType,
                        ServiceLifestyle.Transient,
                        RegistrationType.Try
                    )
                },
                RegistrationFeature.None
            );
        }
    }
}