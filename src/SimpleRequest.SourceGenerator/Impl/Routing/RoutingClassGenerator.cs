using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl.Models;

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
        
        GenerateOrderProperty(classDefinition, moduleEntryPointModel);
        
        _routingTableGenerator.GenerateRoutingTableMethods(
            classDefinition, moduleEntryPointModel, requestModels, context.CancellationToken);
    }

    private void GenerateOrderProperty(ClassDefinition classDefinition, ModuleEntryPointModel moduleEntryPointModel) {
        var orderAttribute = moduleEntryPointModel.AttributeModels.FirstOrDefault(
            a => a.TypeDefinition.Equals(KnownRequestTypes.RoutingOrderAttribute));

        if (orderAttribute is { Arguments.Count: > 0 }) {
            var orderAttributeArgument = orderAttribute.Arguments[0].Value;

            if (orderAttributeArgument != null) {
                var property = classDefinition.AddProperty(typeof(int), "Order");

                property.Get.LambdaSyntax = true;
                property.Get.Add(
                    new WrapStatement(CodeOutputComponent.Get(orderAttributeArgument), "",";"));
                property.Set = null;
            }
        }
    }

    private void GenerateConstructor(ClassDefinition classDefinition) {

        classDefinition.AddComponent(_factory);
        var constructor = classDefinition.AddConstructor();
        var parameter = 
            constructor.AddParameter(KnownRequestTypes.IRequestHandlerFactory, "handlerFactory");
        
        constructor.Assign(parameter).To(_factory.Instance);
    }
}