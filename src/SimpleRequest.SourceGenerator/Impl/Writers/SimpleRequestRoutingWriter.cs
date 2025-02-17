using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Routing;

namespace SimpleRequest.SourceGenerator.Impl.Writers;

public class SimpleRequestRoutingWriter {
    private readonly ITypeDefinition _entryPointAttributeType;
    private readonly DependencyFileWriter _dependencyFileWriter;
    private readonly RoutingClassGenerator _routingClassGenerator;
    private readonly string _routingClassName;

    public SimpleRequestRoutingWriter(ITypeDefinition entryPointAttributeType, string routingClassName) {
        _entryPointAttributeType = entryPointAttributeType;
        _routingClassName = routingClassName;
        _routingClassGenerator = new RoutingClassGenerator(routingClassName);
        _dependencyFileWriter = new DependencyFileWriter();
    }

    public void WriteRouteFile(
        SourceProductionContext context,
        ModuleEntryPointModel entryPointModel,
        DependencyModuleConfigurationModel dependencyModuleConfiguration,
        ImmutableArray<RequestHandlerModel> requestModels) {

        if (requestModels.Length == 0 ||
            !entryPointModel.AttributeModels.Any(
                a => a.ImplementedInterfaces.Contains(KnownRequestTypes.ISimpleRequestEntryAttribute))) {
            return;
        }

        var csharpFile = GenerateCsharpFile(entryPointModel);

        WriteDependencyModuleConfiguration(context, entryPointModel, dependencyModuleConfiguration, requestModels);

        context.CancellationToken.ThrowIfCancellationRequested();

        GenerateCsharpClass(context, entryPointModel, requestModels, csharpFile);

        WriteOutput(context, entryPointModel, csharpFile);
    }

    private void GenerateCsharpClass(
        SourceProductionContext context, ModuleEntryPointModel entryPointModel, ImmutableArray<RequestHandlerModel> requestModels, CSharpFileDefinition csharpFile) {
        var entryClass = csharpFile.AddClass(entryPointModel.EntryPointType.Name);

        entryClass.Modifiers |= ComponentModifier.Public | ComponentModifier.Partial;

        _routingClassGenerator.GenerateRoutingClass(context, entryPointModel, requestModels, entryClass);
    }

    private void WriteDependencyModuleConfiguration(SourceProductionContext context,
        ModuleEntryPointModel entryPointModel,
        DependencyModuleConfigurationModel dependencyModuleConfiguration,
        ImmutableArray<RequestHandlerModel> requestModels) {
        var serviceModels = GenerateServiceModels(requestModels);

        var output =
            _dependencyFileWriter.Write(entryPointModel, dependencyModuleConfiguration, serviceModels, "SimpleRequest");

        context.AddSource(
            $"{entryPointModel.EntryPointType.Name}.SimpleRequestDeps.g.cs",
            output);
    }

    private List<ServiceModel> GenerateServiceModels(ImmutableArray<RequestHandlerModel> requestModels) {
        var handlerTypes = new List<ITypeDefinition>();

        foreach (var requestModel in requestModels) {
            if (!handlerTypes.Contains(requestModel.HandlerType)) {
                handlerTypes.Add(requestModel.HandlerType);
            }
        }

        var serviceModels = new List<ServiceModel>();
        foreach (var requestHandlerModel in handlerTypes) {
            serviceModels.Add(new ServiceModel(
                requestHandlerModel,
                null,
                new ServiceRegistrationModel[] {
                    new(requestHandlerModel, ServiceLifestyle.Transient, RegistrationType.Add)
                }));
        }

        serviceModels.Add(new ServiceModel(
            TypeDefinition.Get("", _routingClassName),
            null,
            new[] {
                new ServiceRegistrationModel(KnownRequestTypes.IRequestHandlerProvider, ServiceLifestyle.Singleton)
            }));

        return serviceModels;
    }

    private void WriteOutput(SourceProductionContext context, ModuleEntryPointModel entryPointModel, CSharpFileDefinition csharpFile) {
        var outputContext = new OutputContext();

        csharpFile.WriteOutput(outputContext);

        context.AddSource(
            $"{entryPointModel.EntryPointType.Name}.SimpleRequestRouting.g.cs",
            outputContext.Output());
    }

    private CSharpFileDefinition GenerateCsharpFile(ModuleEntryPointModel entryPointModel) {
        var csharpFile = new CSharpFileDefinition(entryPointModel.EntryPointType.Namespace);

        return csharpFile;
    }
}