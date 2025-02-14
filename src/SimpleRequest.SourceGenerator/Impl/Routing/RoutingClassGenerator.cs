using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl.Models;
using static CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.SourceGenerator.Impl.Routing;

public class RoutingClassGenerator {
    private readonly RoutingTableGenerator _routingTableGenerator;
    private readonly string _routingClassName;
    private readonly FieldDefinition _factory = new (KnownRequestTypes.IRequestHandlerFactory, "_handlerFactory");

    public RoutingClassGenerator(string routingClassName) {
        _routingClassName = routingClassName;
        _routingTableGenerator = new (_factory);
    }

    public void GenerateRoutingClass(
        SourceProductionContext context,
        ModuleEntryPointModel moduleEntryPointModel, 
        ImmutableArray<RequestHandlerModel> requestModels, 
        ClassDefinition rootClass) {

        var classDefinition = rootClass.AddClass(_routingClassName);
        classDefinition.AddBaseType(KnownRequestTypes.IRequestHandlerProvider);

        GenerateConstructor(classDefinition);
        
        _routingTableGenerator.GenerateGetRequestHandlerMethod(
            classDefinition, moduleEntryPointModel, requestModels, context.CancellationToken);
    }

    private void GenerateConstructor(ClassDefinition classDefinition) {

        classDefinition.AddComponent(_factory);
        var constructor = classDefinition.AddConstructor();
        var parameter = 
            constructor.AddParameter(KnownRequestTypes.IRequestHandlerFactory, "handlerFactory");
        
        constructor.Assign(parameter).To(_factory.Instance);
    }
}