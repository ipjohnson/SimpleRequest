using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl.Models;
using static CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.SourceGenerator.Impl.Writers;

public class FilterAttributeClassGenerator {
    public void WriteClass(SourceProductionContext context, 
        AttributeFilterInfoModel dataLeft) {
        var csharpFile = new CSharpFileDefinition(dataLeft.FilterType.Namespace);

        GenerateFilterAttributeClass(csharpFile, dataLeft);
        
        var output = new OutputContext();
        
        csharpFile.WriteOutput(output);
        
        var outputString = output.Output();

        var uniqueId = 0;
        foreach (var c in dataLeft.FilterType.Namespace) {
            uniqueId += c;
        }
        foreach (var c in dataLeft.FilterType.Name) {
            uniqueId += c;
        }
        
        context.AddSource($"{dataLeft.FilterType.Name}_{uniqueId}.Generated.cs", outputString);
    }

    private void GenerateFilterAttributeClass(CSharpFileDefinition csharpFile, AttributeFilterInfoModel filterInfo) {
        var classDefinition = csharpFile.AddClass(filterInfo.FilterType.Name + "Attribute");

        classDefinition.AddBaseType(TypeDefinition.Get(typeof(Attribute)));
        classDefinition.AddBaseType(KnownRequestTypes.IRequestFilterProvider);


        GenerateProvideMethod(classDefinition, filterInfo);
    }

    private void GenerateProvideMethod(ClassDefinition classDefinition, AttributeFilterInfoModel filterInfo) {
        var method = classDefinition.AddMethod("ProviderFilters");
        
        method.SetReturnType(TypeDefinition.Get("", "IEnumerable<RequestFilterInfo>"));

        method.AddParameter(KnownRequestTypes.IRequestHandlerInfo, "handlerInfo");

        string locateStatement = "";

        if (filterInfo.ServiceModel != null) {
            locateStatement =CreateLocateStatementForDepedencyRegistration(filterInfo, method);
        }
        else {
            
        }

        method.AddUsingNamespace(KnownTypes.Microsoft.DependencyInjection.Namespace);
        method.AddIndentedStatement(
            YieldReturn(
                New(KnownRequestTypes.RequestFilterInfo, 
                    locateStatement,
                    filterInfo.Order)));
    }

    private static string CreateLocateStatementForDepedencyRegistration(AttributeFilterInfoModel filterInfo, MethodDefinition method) {
        string locateStatement = "";

        if (filterInfo.LifeCycle == RequestFilterAttributeLifeCycle.Transient) {
            locateStatement = $"context => context.ServiceProvider.GetRequiredService<{filterInfo.FilterType.Name}>()";
        }
        else {
            method.Assign(Null()).ToLocal(filterInfo.FilterType, "filter");
            
            locateStatement = $"context => filter ??= context.ServiceProvider.GetRequiredService<{filterInfo.FilterType.Name}>()";
        }
        return locateStatement;
    }
}