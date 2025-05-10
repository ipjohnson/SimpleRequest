using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Routing;
using SimpleRequest.SourceGenerator.Impl.Utils;

namespace SimpleRequest.SourceGenerator.Impl.Writers;

public class SimpleRequestRoutingWriter {
    private readonly RoutingClassGenerator _routingClassGenerator;
    private readonly string _routingClassName;
    private readonly string _uniqueId;

    public SimpleRequestRoutingWriter(string routingClassName, string handlerType, string uniqueId) {
        _routingClassName = routingClassName;
        _uniqueId = uniqueId;
        _routingClassGenerator = new RoutingClassGenerator(routingClassName, handlerType);
    }

    public void WriteRouteFile(
        SourceProductionContext context,
        ModuleEntryPointModel entryPointModel,
        DependencyModuleConfigurationModel dependencyModuleConfiguration,
        ImmutableArray<RequestHandlerModel> requestModels) {
        
        if (requestModels.Length == 0) {
            return;
        }

        var basePath = entryPointModel.GetEntryPointBasePath();

        var csharpFile = GenerateCsharpFile(entryPointModel);

        WriteDependencyModuleConfiguration(context, entryPointModel, dependencyModuleConfiguration, requestModels, basePath);

        context.CancellationToken.ThrowIfCancellationRequested();

        GenerateCsharpClass(context, entryPointModel, requestModels, basePath, csharpFile);

        WriteOutput(context, dependencyModuleConfiguration, entryPointModel, csharpFile);
    }

    private void GenerateCsharpClass(SourceProductionContext context, ModuleEntryPointModel entryPointModel, IReadOnlyList<RequestHandlerModel> requestModels, string basePath, CSharpFileDefinition csharpFile) {
        var entryClass = csharpFile.AddClass(entryPointModel.EntryPointType.Name);

        entryClass.Modifiers |= ComponentModifier.Public | ComponentModifier.Partial;

        _routingClassGenerator.GenerateRoutingClass(context, entryPointModel, requestModels, entryClass);
    }

    private void WriteDependencyModuleConfiguration(SourceProductionContext context,
        ModuleEntryPointModel entryPointModel,
        DependencyModuleConfigurationModel dependencyModuleConfiguration,
        IReadOnlyList<RequestHandlerModel> requestModels, string basePath) {
        var serviceModels = GenerateServiceModels(requestModels);

        using var fileLogger = new FileLogger(dependencyModuleConfiguration, "SimpleRequestModule");
        var dependencyFileWriter = new DependencyFileWriter(fileLogger);
        var output =
            dependencyFileWriter.Write(entryPointModel, dependencyModuleConfiguration, serviceModels, "SimpleRequest" + _uniqueId);

        context.AddSource(
            entryPointModel.EntryPointType.GetFileNameHint(
                dependencyModuleConfiguration.RootNamespace,
                "RequestDeps." +_uniqueId
                ),           output);
    }

    private List<ServiceModel> GenerateServiceModels(IReadOnlyList<RequestHandlerModel> requestModels) {
        var handlerTypes = new Dictionary<ITypeDefinition, ConstructorInfoModel?>();
        foreach (var requestModel in requestModels) {
            if (!handlerTypes.ContainsKey(requestModel.HandlerType)) {
                handlerTypes[requestModel.HandlerType] = requestModel.ConstructorInfo;
            }
        }

        var serviceModels = new List<ServiceModel>();
        foreach (var kvp in handlerTypes) {
            serviceModels.Add(new ServiceModel(
                kvp.Key,
                kvp.Value,
                null,
                null,
                new ServiceRegistrationModel[] {
                    new(kvp.Key, ServiceLifestyle.Transient, RegistrationType.Add)
                }, RegistrationFeature.None));
        }

        serviceModels.Add(new ServiceModel(
            TypeDefinition.Get("", _routingClassName),
            null,
            null,
            null,
            new[] {
                new ServiceRegistrationModel(KnownRequestTypes.IRequestHandlerProvider, ServiceLifestyle.Singleton)
            }, RegistrationFeature.None));

        return serviceModels;
    }

    private void WriteOutput(SourceProductionContext context,
        DependencyModuleConfigurationModel dependencyModuleConfiguration,
        ModuleEntryPointModel entryPointModel,
        CSharpFileDefinition csharpFile) {
        var outputContext = new OutputContext(
            OutputContextDefault.Instance
        );

        csharpFile.WriteOutput(outputContext);

        var output = outputContext.Output();
        context.AddSource(
            entryPointModel.EntryPointType.GetFileNameHint(
                dependencyModuleConfiguration.RootNamespace,
                "Routing" + _uniqueId),
            output);
    }

    private CSharpFileDefinition GenerateCsharpFile(ModuleEntryPointModel entryPointModel) {
        var csharpFile = new CSharpFileDefinition(entryPointModel.EntryPointType.Namespace);

        return csharpFile;
    }
}