using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.SourceGenerator.Impl;

public class RequestHandlerCollectionModelGenerator {

    public RequestHandlerCollection GenerateModel(GeneratorSyntaxContext context, CancellationToken cancellationToken) {
        var classDeclarationSyntax = context.Node as ClassDeclarationSyntax;

        var handlers = new List<RequestHandlerModel>();

        if (classDeclarationSyntax != null &&
            (classDeclarationSyntax.BaseList?.Types.Any() ?? false)) {

            var handlerType = GetControllerType(context, classDeclarationSyntax);

            var classAttributeModels = 
                GetClassAttributeModels(context, classDeclarationSyntax, cancellationToken);

            foreach (var baseType in classDeclarationSyntax.BaseList.Types) {
                cancellationToken.ThrowIfCancellationRequested();

                var interfaceType = baseType.Type.GetTypeDefinition(context);

                if (interfaceType is { TypeDefinitionEnum: TypeDefinitionEnum.InterfaceDefinition }) {
                    var semanticModel = context.SemanticModel.GetSymbolInfo(baseType.Type, cancellationToken);

                    if (semanticModel.Symbol is INamedTypeSymbol namedTypeSymbol) {

                        ProcessInterfaceMembers(
                            context, 
                            cancellationToken, 
                            namedTypeSymbol, 
                            classDeclarationSyntax,
                            handlerType,
                            classAttributeModels,
                            interfaceType,
                            handlers);

                        foreach (var typeSymbol in namedTypeSymbol.AllInterfaces) {
                            ProcessInterfaceMembers(
                                context, 
                                cancellationToken, 
                                typeSymbol, 
                                classDeclarationSyntax,
                                handlerType,
                                classAttributeModels,
                                interfaceType,
                                handlers);
                        }
                    }
                }
            }

        }

        return new RequestHandlerCollection(handlers);
    }

    private void ProcessInterfaceMembers(GeneratorSyntaxContext context, CancellationToken cancellationToken, INamedTypeSymbol namedTypeSymbol, ClassDeclarationSyntax classDeclarationSyntax,
        ITypeDefinition handlerType, IReadOnlyList<AttributeModel> classAttributeModels, ITypeDefinition interfaceType, List<RequestHandlerModel> handlers) {
        foreach (var member in namedTypeSymbol.GetMembers()) {
            cancellationToken.ThrowIfCancellationRequested();
            if (member is IMethodSymbol interfaceMethodSymbol) {

                GetHandlerInfoFromMember(
                    context,
                    classDeclarationSyntax,
                    handlerType,
                    classAttributeModels,
                    interfaceType,
                    interfaceMethodSymbol,
                    handlers,
                    cancellationToken);
            }
        }
    }

    private IReadOnlyList<AttributeModel> GetClassAttributeModels(GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax, CancellationToken cancellationToken) {

        return AttributeModelHelper.GetAttributeModels(context, classDeclarationSyntax, cancellationToken);
    }
    
    private ITypeDefinition GetControllerType(GeneratorSyntaxContext context, ClassDeclarationSyntax classDeclarationSyntax) {

        var namespaceSyntax = classDeclarationSyntax.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>().First();

        return TypeDefinition.Get(namespaceSyntax.Name.ToFullString().TrimEnd(),
            classDeclarationSyntax.Identifier.Text);
    }

    private void GetHandlerInfoFromMember(GeneratorSyntaxContext context,
        ClassDeclarationSyntax classDeclarationSyntax,
        ITypeDefinition handlerType,
        IReadOnlyList<AttributeModel> classAttributeModels,
        ITypeDefinition interfaceType,
        IMethodSymbol interfaceMethodSymbol,
        List<RequestHandlerModel> handlers,
        CancellationToken cancellationToken) {

        var attributes = interfaceMethodSymbol.GetAttributes();

        var implementationMethodDeclaration = GetImplementationMethodDeclaration(classDeclarationSyntax, interfaceMethodSymbol);

        bool found = false;
        foreach (var attributeData in attributes) {
            cancellationToken.ThrowIfCancellationRequested();

            var handler = ProcessAttribute(
                context,
                classDeclarationSyntax,
                handlerType,
                classAttributeModels,
                interfaceType,
                implementationMethodDeclaration,
                interfaceMethodSymbol,
                attributeData);

            if (handler != null) {
                handlers.Add(handler);
                found = true;
            }
        }

        if (!found) {
            var handler = GetRequestHandlerFromSymbol(
                context,
                classDeclarationSyntax,
                handlerType,
                classAttributeModels,
                interfaceType,
                implementationMethodDeclaration,
                interfaceMethodSymbol,
                null,
                null);
        }
    }

    private MethodDeclarationSyntax GetImplementationMethodDeclaration(ClassDeclarationSyntax classDeclarationSyntax, IMethodSymbol methodSymbol) {
        var methods = classDeclarationSyntax.Members.OfType<MethodDeclarationSyntax>();

        // todo: tighten logic
        return methods.First(method => method.Identifier.Text == methodSymbol.Name);
    }

    private RequestHandlerModel? ProcessAttribute(GeneratorSyntaxContext context,
        ClassDeclarationSyntax classDeclarationSyntax,
        ITypeDefinition handlerType,
        IReadOnlyList<AttributeModel> classAttributeModels,
        ITypeDefinition interfaceType,
        MethodDeclarationSyntax implementationMethodDeclaration,
        IMethodSymbol methodSymbol,
        AttributeData attributeData) {
        if (attributeData.AttributeClass == null) {
            return null;
        }

        var attributeType = attributeData.AttributeClass.GetTypeDefinition();

        if (!RequestAttributeSourceGenerator.RequestAttributeTypes.Contains(attributeType)) {
            return null;
        }

        return GetRequestHandlerFromSymbol(
            context,
            classDeclarationSyntax,
            handlerType,
            classAttributeModels,
            interfaceType,
            implementationMethodDeclaration,
            methodSymbol,
            attributeData,
            attributeType);
    }

    private RequestHandlerModel GetRequestHandlerFromSymbol(GeneratorSyntaxContext context,
        ClassDeclarationSyntax classDeclarationSyntax,
        ITypeDefinition handlerType,
        IReadOnlyList<AttributeModel> classAttributeModels,
        ITypeDefinition interfaceType,
        MethodDeclarationSyntax implementationMethodDeclaration,
        IMethodSymbol interfaceMethodSymbol,
        AttributeData? attributeData,
        ITypeDefinition? attributeType) {

        var modelName = attributeData == null ? 
            GetModelFromInterfaceName(interfaceType, interfaceMethodSymbol) :
            GetModelName(interfaceMethodSymbol, attributeData, attributeType!);
        
        return new RequestHandlerModel(
            modelName,
            handlerType,
            interfaceMethodSymbol.Name,
            null,
            TypeDefinition.Get(handlerType.Namespace + ".Generated", handlerType.Name + "_" + interfaceMethodSymbol.Name),
            GetRequestParameters(modelName, interfaceMethodSymbol),
            GetResponseModel(context, interfaceMethodSymbol),
            GetAllHandlerAttributes(classDeclarationSyntax, implementationMethodDeclaration, interfaceMethodSymbol, classAttributeModels)
        );
    }

    private RequestHandlerNameModel GetModelFromInterfaceName(ITypeDefinition interfaceType, IMethodSymbol interfaceMethodSymbol) {
        return new RequestHandlerNameModel(
            $"/{interfaceType.Name}/{interfaceMethodSymbol.Name}", "POST"
        );
    }

    private IReadOnlyList<AttributeModel> GetAllHandlerAttributes(ClassDeclarationSyntax classDeclarationSyntax, MethodDeclarationSyntax implementationMethodDeclaration, IMethodSymbol interfaceMethodSymbol,
        IReadOnlyList<AttributeModel> classAttributeModels) {
        return classAttributeModels;
    }

    private ResponseInformationModel GetResponseModel(GeneratorSyntaxContext context, IMethodSymbol interfaceMethodSymbol) {
        var returnType = interfaceMethodSymbol.ReturnType.GetTypeDefinition();

        return new ResponseInformationModel(
            returnType.Namespace.Equals("System.Threading.Tasks") &&
            returnType.Name.EndsWith("Task"),
            returnType,
            null,
            null,
            null
        );
    }

    private IReadOnlyList<RequestParameterInformation> GetRequestParameters(RequestHandlerNameModel modelName, IMethodSymbol interfaceMethodSymbol) {
        var parameterInfos = new List<RequestParameterInformation>();

        if (interfaceMethodSymbol.Parameters == null) {
            return parameterInfos;
        }

        var parameterIndex = 0;

        var pathHasParameters = modelName.Path.IndexOf('{') >= 0;

        foreach (var parameter in interfaceMethodSymbol.Parameters) {
            var parameterType = parameter.Type.GetTypeDefinition();
            var parameterAttributes = GetParameterAttributes(parameter);

            var (bindingType, bindingName) = GetBindingInfo(modelName, parameter, parameterType, parameterAttributes);

            parameterInfos.Add(new RequestParameterInformation(
                parameterType,
                parameter.Name,
                parameter.IsOptional,
                parameter.HasExplicitDefaultValue ? parameter.ExplicitDefaultValue : null,
                bindingType,
                bindingName,
                parameterIndex,
                parameterAttributes
            ));
        }

        return parameterInfos;
    }

    private (ParameterBindType bindingType, string bindingName) GetBindingInfo(RequestHandlerNameModel modelName,
        IParameterSymbol parameter,
        ITypeDefinition parameterType,
        IReadOnlyList<AttributeModel> parameterAttributes) {

        ParameterBindType? bindingType = null;
        string bindingName = "";

        foreach (var attributeModel in parameterAttributes) {
            if (attributeModel.TypeDefinition.Equals(KnownRequestTypes.Attributes.FromBody)) {
                bindingType = ParameterBindType.Body;
            }
            else if (attributeModel.TypeDefinition.Equals(KnownRequestTypes.Attributes.FromQuery)) {
                bindingType = ParameterBindType.QueryString;
                bindingName = GetBindingName(attributeModel, parameter.Name);
            }
            else if (attributeModel.TypeDefinition.Equals(KnownRequestTypes.Attributes.FromHeader)) {
                bindingType = ParameterBindType.Header;
                bindingName = GetBindingName(attributeModel, parameter.Name);
            }
            else if (attributeModel.TypeDefinition.Equals(KnownRequestTypes.Attributes.FromCookie)) {
                bindingType = ParameterBindType.Cookie;
                bindingName = GetBindingName(attributeModel, parameter.Name);
            }
            else if (attributeModel.TypeDefinition.Equals(KnownRequestTypes.Attributes.FromServices)) {
                bindingType = ParameterBindType.FromServiceProvider;
            }
            else if (attributeModel.ImplementedInterfaces.Contains(KnownRequestTypes.IInvokeParameterValueProvider)) {
                bindingType = ParameterBindType.CustomAttribute;
            }
        }

        if (bindingType == null) {
            if (modelName.Path.Contains($"{{{parameter.Name}}}")) {
                bindingType = ParameterBindType.Path;
                bindingName = parameter.Name;
            }
        }

        if (!bindingType.HasValue) {
            bindingType = ParameterBindType.Body;
        }

        return (bindingType.Value, bindingName);
    }

    private string GetBindingName(AttributeModel attributeModel, string parameterName) {
        if (attributeModel.Arguments.Count == 1) {
            return attributeModel.Arguments[0].Value?.ToString() ?? parameterName;
        }

        return parameterName;
    }

    private IReadOnlyList<AttributeModel> GetParameterAttributes(IParameterSymbol parameter) {
        var attributeModels = new List<AttributeModel>();

        return attributeModels;
    }

    private RequestHandlerNameModel GetModelName(
        IMethodSymbol interfaceMethodSymbol, AttributeData attributeData, ITypeDefinition attributeType) {

        if (attributeType.Equals(KnownRequestTypes.Attributes.Function)) {
            return GetFunctionAttributeName(interfaceMethodSymbol, attributeData);
        }

        if (attributeType.Equals(KnownRequestTypes.Attributes.Http)) {
            return HttpAttributeName(interfaceMethodSymbol, attributeData);
        }

        return HttpVerbAttributeName(interfaceMethodSymbol, attributeData, attributeType);
    }

    private RequestHandlerNameModel HttpVerbAttributeName(IMethodSymbol interfaceMethodSymbol, AttributeData attributeData, ITypeDefinition attributeType) {
        var method = "POST";
        var path = "/";

        if (attributeType.Equals(KnownRequestTypes.Attributes.Get)) {
            method = "GET";
        }
        else if (attributeType.Equals(KnownRequestTypes.Attributes.Put)) {
            method = "PUT";
        }
        else if (attributeType.Equals(KnownRequestTypes.Attributes.Patch)) {
            method = "PATCH";
        }
        else if (attributeType.Equals(KnownRequestTypes.Attributes.Delete)) {
            method = "DELETE";
        }
        else if (attributeType.Equals(KnownRequestTypes.Attributes.Head)) {
            method = "HEAD";
        }

        if (attributeData.ConstructorArguments.Length > 0) {
            path = attributeData.ConstructorArguments[0].Value?.ToString() ?? path;
        }

        return new RequestHandlerNameModel(path, method);
    }

    private RequestHandlerNameModel HttpAttributeName(IMethodSymbol interfaceMethodSymbol, AttributeData attributeData) {
        var method = "POST";
        var path = "/";

        for (var i = 0; i < attributeData.ConstructorArguments.Length; i++) {
            if (i == 0) {
                method = attributeData.ConstructorArguments[i].Value?.ToString() ?? method;
            }
            if (i == 1) {
                path = attributeData.ConstructorArguments[i].Value?.ToString() ?? path;
            }
        }

        return new RequestHandlerNameModel(path, method);
    }

    private RequestHandlerNameModel GetFunctionAttributeName(IMethodSymbol interfaceMethodSymbol, AttributeData attributeData) {
        var method = "POST";
        var path = "/";

        if (attributeData.NamedArguments.Length > 0) {
            path = attributeData.NamedArguments[0].Value.ToString() ?? path;
        }

        return new RequestHandlerNameModel(path, method);
    }
}