using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Models;
using SimpleRequest.SourceGenerator.Impl.Writers;
using ISourceGenerator = DependencyModules.SourceGenerator.Impl.ISourceGenerator;

namespace SimpleRequest.SourceGenerator.Impl;

public class FilterAttributeSourceGenerator : ISourceGenerator {
    private readonly ITypeDefinition _entryPointAttributeType;
    private readonly string _uniqueName;

    public FilterAttributeSourceGenerator(ITypeDefinition entryPointAttributeType, string uniqueName) {
        _entryPointAttributeType = entryPointAttributeType;
        _uniqueName = uniqueName;
    }

    public void SetupGenerator(
        IncrementalGeneratorInitializationContext context,
        IncrementalValuesProvider<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> incrementalValueProvider) {
        var methodSelector = new SyntaxSelector<ClassDeclarationSyntax>(
            KnownRequestTypes.RequestFilterAttribute
        );

        var requestModelProvider = context.SyntaxProvider.CreateSyntaxProvider(
            methodSelector.Where,
            GenerateFilterModel
        ).WithComparer(GetModelComparer());

        var collection =
            requestModelProvider.Collect();

        context.RegisterSourceOutput(
            incrementalValueProvider.Combine(collection),
            GenerateFilterRegistrations
        );

        var modelEntryCollection =
            incrementalValueProvider.Collect();

        context.RegisterSourceOutput(
            requestModelProvider.Combine(modelEntryCollection),
            GenerateFilterAttributes
        );
    }

    private void GenerateFilterAttributes(SourceProductionContext context,
        (AttributeFilterInfoModel Left, ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> Right) data) {
        var generator = new FilterAttributeClassGenerator();

        generator.WriteClass(context, data.Left);
    }

    private void GenerateFilterRegistrations(SourceProductionContext context,
        ((ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right) Left, ImmutableArray<AttributeFilterInfoModel> Right) data) {
        if (data.Left.Left.AttributeModels.Count == 0 ||
            !data.Left.Left.AttributeModels.Any(
                a => a.ImplementedInterfaces.Contains(KnownRequestTypes.ISimpleRequestEntryAttribute))) {
            return;
        }

        WriteAttributeRegistrations(context, data);
    }

    private void WriteAttributeRegistrations(SourceProductionContext context, ((ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right) Left, ImmutableArray<AttributeFilterInfoModel> Right) data) {
        if (data.Right.Length == 0) {
            return;
        }
        
        var serviceModels =
            data.Right.Select(
                model => new ServiceModel(
                    model.FilterType,
                    null,
                    new[] {
                        new ServiceRegistrationModel(model.FilterType, ServiceLifestyle.Transient)
                    }));
        
        var writeString =
            new DependencyFileWriter().Write(data.Left.Left, data.Left.Right, serviceModels, _uniqueName);

        context.AddSource($"{data.Left.Left.EntryPointType.Name}.{_uniqueName}.g.cs", writeString);
    }

    private IEqualityComparer<AttributeFilterInfoModel> GetModelComparer() {
        return new AttributeFilterInfoModelComparer();
    }

    private AttributeFilterInfoModel GenerateFilterModel(GeneratorSyntaxContext context, CancellationToken cancellation) {
        var lifecycle = RequestFilterAttributeLifeCycle.Transient;
        var order = 100;
        ServiceModel? serviceModel = null;

        if (context.Node is ClassDeclarationSyntax classDeclarationSyntax) {
            var attributes = AttributeModelHelper
                .GetAttributes(context, classDeclarationSyntax.AttributeLists, cancellation)
                .ToList();

            var (lc, o, sm) = GetLifecycleFlags(context, cancellation, attributes);

            if (lc.HasValue) {
                lifecycle = lc.Value;
            }
            if (o.HasValue) {
                order = o.Value;
            }
            if (sm != null) {
                serviceModel = sm;
            }
        }

        var classInfo = AttributeModelHelper.GetAttributeClassInfo(context, cancellation);
        
        return new AttributeFilterInfoModel(
            GetClassDefinition(context),
            lifecycle,
            order,
            serviceModel,
            classInfo.ConstructorParameters,
            classInfo.Properties
        );
    }

    private (RequestFilterAttributeLifeCycle?, int?, ServiceModel?) GetLifecycleFlags(
        GeneratorSyntaxContext context, CancellationToken cancellation, List<AttributeModel> attributes) {
        RequestFilterAttributeLifeCycle? lifecycle = null;
        int? order = null;

        foreach (var attributeModel in attributes) {
            if (attributeModel.TypeDefinition.Equals(KnownRequestTypes.RequestFilterAttribute)) {
                foreach (var property in attributeModel.Properties) {
                    if (property.Name == "Order" && property.Value is int intValue) {
                        order = intValue;
                    }
                    else if (property.Name == "Reuse") {
                        lifecycle = true.Equals(property.Value) ? RequestFilterAttributeLifeCycle.Reuse : RequestFilterAttributeLifeCycle.Transient;
                    }
                }
            }
        }

        return (lifecycle, order, ServiceModelUtility.GetServiceModel(context, cancellation));
    }

    private ITypeDefinition GetClassDefinition(GeneratorSyntaxContext context) {
        ITypeDefinition classTypeDefinition;

        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        if (classDeclarationSyntax.TypeParameterList is { Parameters.Count: > 0 }) {
            classTypeDefinition =
                new GenericTypeDefinition(
                    TypeDefinitionEnum.ClassDefinition,
                    classDeclarationSyntax.GetNamespace(),
                    classDeclarationSyntax.Identifier.ToString(),
                    classDeclarationSyntax.TypeParameterList.Parameters.Select(
                        _ => TypeDefinition.Get("", "")).ToArray()
                );
        }
        else {
            classTypeDefinition = TypeDefinition.Get(classDeclarationSyntax.GetNamespace(),
                classDeclarationSyntax.Identifier.ToString());
        }

        return classTypeDefinition;
    }
}