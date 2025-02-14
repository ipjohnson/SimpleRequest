using System.Collections.Immutable;
using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using Microsoft.CodeAnalysis;
using SimpleRequest.SourceGenerator.Impl.Models;
using static CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.SourceGenerator.Impl.Writers;

public class SimpleRequestHandlerWriter {
    private readonly BindParameterMethodGenerator _bindParameterMethodGenerator = new();
    private readonly ParameterTypeGenerator _parameterTypeGenerator = new();
    private readonly InvokeMethodGenerator _invokeMethodGenerator = new();

    public void WriteRequestFile(
        SourceProductionContext context,
        RequestHandlerModel requestModel,
        ImmutableArray<(ModuleEntryPointModel Left, DependencyModuleConfigurationModel Right)> entryPointModels) {

        var outputFile = GenerateCsharpFile(requestModel);

        GenerateHandlerClass(outputFile, requestModel);

        WriteOutputFile(context, requestModel, outputFile);
    }

    private void WriteOutputFile(
        SourceProductionContext context,
        RequestHandlerModel requestModel,
        CSharpFileDefinition outputFile) {

        var outputContext = new OutputContext();

        outputFile.WriteOutput(outputContext);

        context.AddSource(
            $"{requestModel.GenerateInvokeType.Name}.g.cs", outputContext.Output());
    }

    private void GenerateHandlerClass(CSharpFileDefinition outputFile, RequestHandlerModel requestModel) {
        var classDefinition = outputFile.AddClass(requestModel.GenerateInvokeType.Name);
        classDefinition.Modifiers |= ComponentModifier.Static;
        classDefinition.WrapInPragma("1998");

        var requestParameterType =
            _parameterTypeGenerator.GenerateParameterType(requestModel, classDefinition);

        ConstructParameterInfoCreationMethod(requestModel, classDefinition, requestParameterType);

        _bindParameterMethodGenerator.GenerateBindParameterMethod(requestModel, classDefinition, requestParameterType);

        _invokeMethodGenerator.GenerateInvokeMethod(requestModel, classDefinition, requestParameterType);

        ConstructInvokeHandlerInfo(requestModel, classDefinition);
    }

    private PropertyDefinition ConstructInvokeHandlerInfo(RequestHandlerModel requestModel, ClassDefinition classDefinition) {
        var property = classDefinition.AddProperty(KnownRequestTypes.IRequestHandlerInfo, "HandlerInfo");

        property.Modifiers |= ComponentModifier.Static | ComponentModifier.Public;
        property.Set = null;

        var newInvoke = New(
            KnownRequestTypes.RequestHandlerInfoMethods,
            QuoteString(requestModel.HandlerMethod),
            "Invoke",
            "CreateParameters",
            "BindParameterInfo",
            "InvokeParameters.ParameterInfoField"
        );
        
        var attributeArray = GetAttributeArray(requestModel);

        var newStatement = New(
            KnownRequestTypes.RequestHandlerInfo,
            QuoteString(requestModel.Name.Path),
            QuoteString(requestModel.Name.Method),
            TypeOf(requestModel.HandlerType),
            Null(),
            Null(),
            Null(),
            attributeArray,
            newInvoke);
        
        property.Get.Return(newStatement);

        return property;
    }

    private IOutputComponent GetAttributeArray(RequestHandlerModel requestModel) {
        var attributeStatements = new List<object>();

        foreach (var attributeModel in requestModel.Filters) {
            var newStatement = New(attributeModel.TypeDefinition, attributeModel.ArgumentString);

            if (attributeModel.Properties.Count > 0) {
                newStatement.AddInitValue(attributeModel.PropertyString);
            }
            
            attributeStatements.Add(newStatement);
        }

        if (attributeStatements.Count > 0) {
            return NewArray(typeof(Attribute), attributeStatements.ToArray());
        }
        
        return NewArray(typeof(Attribute), 0);
    }

    private void ConstructParameterInfoCreationMethod(
        RequestHandlerModel requestModel, ClassDefinition classDefinition, ITypeDefinition requestParameterType) {
        var method = classDefinition.AddMethod("CreateParameters");
        method.Modifiers |= ComponentModifier.Static | ComponentModifier.Private;
        method.SetReturnType(KnownRequestTypes.IInvokeParameters);
        method.AddParameter(KnownRequestTypes.IRequestContext, "context");

        method.Return(SyntaxHelpers.New(requestParameterType));
    }

    private CSharpFileDefinition GenerateCsharpFile(RequestHandlerModel requestModel) {
        var namespaceStr = requestModel.GenerateInvokeType.Namespace;

        return new CSharpFileDefinition(namespaceStr);
    }
}