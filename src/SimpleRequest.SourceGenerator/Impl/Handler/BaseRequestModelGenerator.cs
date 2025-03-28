using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl;
using DependencyModules.SourceGenerator.Impl.Models;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SimpleRequest.SourceGenerator.Impl.Models;

namespace SimpleRequest.SourceGenerator.Impl.Handler;

public abstract class BaseRequestModelGenerator {
    public virtual RequestHandlerModel GenerateRequestModel(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        var methodDeclaration = (MethodDeclarationSyntax)context.Node;

        var attributeModules = AttributeModelHelper.GetAttributeModels(context, methodDeclaration, cancellationToken);

        var methodName = GetControllerMethod(methodDeclaration);
        var controllerType = GetControllerType(methodDeclaration);

        var response = GetResponseInformation(context, methodDeclaration);
        var filters = GetFilters(context, methodDeclaration, attributeModules, cancellationToken);

        var nameModel = GetRequestNameModel(context, methodDeclaration, attributeModules, cancellationToken);

        return new RequestHandlerModel(
            nameModel,
            controllerType,
            methodName,
            ServiceModelUtility.GetConstructorInfo(context, cancellationToken),
            GetInvokeHandlerType(context, methodDeclaration, cancellationToken),
            GetParameters(context, methodDeclaration, nameModel, cancellationToken),
            response,
            filters);
    }

    protected abstract RequestHandlerNameModel GetRequestNameModel(GeneratorSyntaxContext context,
        MethodDeclarationSyntax methodDeclaration,
        IReadOnlyList<AttributeModel> attributeModules,
        CancellationToken cancellation);

    protected virtual ITypeDefinition GetInvokeHandlerType(
        GeneratorSyntaxContext context,
        MethodDeclarationSyntax methodDeclaration,
        CancellationToken cancellation) {

        var classDeclarationSyntax =
            methodDeclaration.Ancestors().OfType<ClassDeclarationSyntax>().First();

        var namespaceSyntax = classDeclarationSyntax.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().First();

        var className = classDeclarationSyntax.Identifier + "_" + methodDeclaration.Identifier.Text;

        if (methodDeclaration.ParameterList.Parameters.Count > 0) {
            var parameterString = "";

            foreach (var parameter in methodDeclaration.ParameterList.Parameters) {
                parameterString += '|' + parameter.Identifier.Text;
            }

            className += "_" + parameterString.Select(c => (int)c).Aggregate((total, c) => total + c);
        }


        return TypeDefinition.Get(namespaceSyntax.Name.ToFullString().TrimEnd() + ".Generated", className);
    }

    protected virtual IReadOnlyList<RequestParameterInformation> GetParameters(
        GeneratorSyntaxContext generatorSyntaxContext,
        MethodDeclarationSyntax methodDeclaration,
        RequestHandlerNameModel requestHandlerNameModel,
        CancellationToken cancellationToken) {
        var parameters = new List<RequestParameterInformation>();
        for (var i = 0; i < methodDeclaration.ParameterList.Parameters.Count; i++) {
            var parameter = methodDeclaration.ParameterList.Parameters[i];
            cancellationToken.ThrowIfCancellationRequested();

            var attributeModels =
                AttributeModelHelper.GetAttributeModels(generatorSyntaxContext, parameter, CancellationToken.None);

            RequestParameterInformation? parameterInformation =
                GetParameterInfoFromAttributes(generatorSyntaxContext, methodDeclaration,
                    requestHandlerNameModel,
                    parameter,
                    attributeModels,
                    i);

            if (parameterInformation == null) {
                parameterInformation = GetParameterInfo(
                    generatorSyntaxContext,
                    methodDeclaration,
                    requestHandlerNameModel,
                    parameter,
                    attributeModels,
                    i);
            }

            parameters.Add(parameterInformation);
        }

        return parameters;
    }

    protected virtual RequestParameterInformation? DefaultGetParameterFromAttribute(
        AttributeModel attributeModel,
        GeneratorSyntaxContext generatorSyntaxContext,
        ParameterSyntax parameter,
        int parameterIndex,
        IReadOnlyList<AttributeModel> parameterAttributes) {
        var parameterType = parameter.Type?.GetTypeDefinition(generatorSyntaxContext)!;
        var name = parameter.Identifier.Text;

        if (!attributeModel.ImplementedInterfaces.Contains(KnownRequestTypes.IInvokeParameterValueProvider)) {
            return null;
        }

        string? defaultValue = null;

        if (parameter.Default != null) {
            defaultValue = parameter.Default.Value.ToFullString();
        }

        return new RequestParameterInformation(
            parameterType,
            name,
            !parameterType.IsNullable,
            defaultValue,
            ParameterBindType.CustomAttribute,
            "",
            parameterIndex,
            parameterAttributes
        );
    }


    protected virtual RequestParameterInformation GetParameterInfo(GeneratorSyntaxContext generatorSyntaxContext,
        MethodDeclarationSyntax methodDeclarationSyntax,
        RequestHandlerNameModel requestHandlerNameModel,
        ParameterSyntax parameter,
        IReadOnlyList<AttributeModel> attributeModels,
        int parameterIndex) {
        var parameterType = parameter.Type?.GetTypeDefinition(generatorSyntaxContext)!;

        if (KnownRequestTypes.IRequestContext.Equals(parameterType)) {
            return CreateRequestParameterInformation(parameter, parameterType,
                ParameterBindType.RequestContext,
                parameterIndex,
                true,
                null,
                attributeModels);
        }

        if (KnownRequestTypes.IRequestData.Equals(parameterType)) {
            return CreateRequestParameterInformation(parameter, parameterType,
                ParameterBindType.RequestData,
                parameterIndex,
                true,
                null,
                attributeModels);
        }

        if (KnownRequestTypes.IResponseData.Equals(parameterType)) {
            return CreateRequestParameterInformation(parameter, parameterType,
                ParameterBindType.ResponseData,
                parameterIndex,
                true,
                null,
                attributeModels);
        }

        if (KnownTypes.Microsoft.DependencyInjection.IServiceProvider.Equals(parameterType)) {
            return CreateRequestParameterInformation(parameter,
                parameterType,
                ParameterBindType.ServiceProvider,
                parameterIndex,
                null,
                null,
                attributeModels);
        }

        if (parameterType.TypeDefinitionEnum == TypeDefinitionEnum.InterfaceDefinition) {
            return CreateRequestParameterInformation(parameter,
                parameterType,
                ParameterBindType.FromServiceProvider,
                parameterIndex,
                null,
                null,
                attributeModels);
        }

        foreach (var attributeModel in attributeModels) {
            ParameterBindType? bindType = null;

            switch (attributeModel.TypeDefinition.Name) {
                case "FromServicesAttribute":
                    return CreateRequestParameterInformation(parameter, parameterType,
                        ParameterBindType.FromServiceProvider, parameterIndex);
                case "FromHeaderAttribute":
                    bindType = ParameterBindType.Header;
                    break;
                case "FromQueryAttribute":
                    bindType = ParameterBindType.QueryString;
                    break;
                case "FromCookieAttribute":
                    bindType = ParameterBindType.Cookie;
                    break;
            }

            if (bindType != null) {
                var argument = attributeModel.Arguments.FirstOrDefault()?.Value?.ToString();

                var bindingName = string.IsNullOrEmpty(argument) ? parameter.Identifier.ToString() : argument;

                return CreateRequestParameterInformation(parameter,
                    parameterType,
                    bindType.Value,
                    parameterIndex,
                    null,
                    bindingName,
                    attributeModels);
            }
        }

        var id = parameter.Identifier.Text;

        if (requestHandlerNameModel.Path.Contains($"{{{id}}}")) {
            return CreateRequestParameterInformation(parameter, parameterType,
                ParameterBindType.Path,
                parameterIndex,
                null,
                id,
                attributeModels);
        }

        return CreateRequestParameterInformation(
            parameter,
            parameterType,
            ParameterBindType.Body,
            parameterIndex,
            null,
            null,
            attributeModels);
    }

    public static RequestParameterInformation CreateRequestParameterInformation(
        ParameterSyntax parameter,
        ITypeDefinition parameterType,
        ParameterBindType parameterBindType,
        int parameterIndex,
        bool? required = null,
        string? bindingName = null,
        IReadOnlyList<AttributeModel>? customAttributes = default) {
        if (!parameterType.IsNullable && parameter.ToFullString().Contains("?")) {
            parameterType = parameterType.MakeNullable();
        }

        string? defaultValue = null;

        if (parameter.Default != null) {
            defaultValue = parameter.Default.Value.ToFullString();
        }

        return new RequestParameterInformation(
            parameterType,
            parameter.Identifier.Text,
            required ?? !parameterType.IsNullable,
            defaultValue,
            parameterBindType,
            bindingName ?? string.Empty,
            parameterIndex,
            customAttributes ?? Array.Empty<AttributeModel>());
    }

    protected abstract RequestParameterInformation? GetParameterInfoFromAttributes(GeneratorSyntaxContext generatorSyntaxContext,
        MethodDeclarationSyntax methodDeclarationSyntax,
        RequestHandlerNameModel requestHandlerNameModel,
        ParameterSyntax parameter,
        IReadOnlyList<AttributeModel> attributeModels,
        int parameterIndex);

    protected virtual string GetControllerMethod(MethodDeclarationSyntax methodDeclaration) {
        return methodDeclaration.Identifier.Text;
    }

    protected virtual ITypeDefinition GetControllerType(SyntaxNode contextNode) {
        var classDeclarationSyntax =
            contextNode.Ancestors().OfType<ClassDeclarationSyntax>().First();

        var namespaceSyntax = classDeclarationSyntax.Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>().First();

        return TypeDefinition.Get(namespaceSyntax.Name.ToFullString().TrimEnd(),
            classDeclarationSyntax.Identifier.Text);
    }

    protected virtual ResponseInformationModel GetResponseInformation(
        GeneratorSyntaxContext context,
        MethodDeclarationSyntax methodDeclaration) {
        var templateAttribute = context.Node.GetAttribute("Template");
        var template = "";

        if (templateAttribute is { ArgumentList.Arguments.Count: > 0 }) {
            template = templateAttribute.ArgumentList.Arguments[0].ToString().Trim('"');
        }

        var returnType = methodDeclaration.ReturnType.GetTypeDefinition(context);

        var isAsync = false;

        if (returnType is GenericTypeDefinition genericType) {
            isAsync = genericType.Name.Equals("Task") || genericType.Name.Equals("ValueTask");
        }
        else if (returnType?.Name == "Task") {
            isAsync = true;
        }

        var rawResponse = "";
        var varResponseAttribute = context.Node.GetAttribute("RawResponse");

        if (varResponseAttribute != null) {
            rawResponse =
                varResponseAttribute.ArgumentList?.Arguments[0].ToString().Trim('"') ??
                "text/plain";
        }

        return new ResponseInformationModel(isAsync, returnType, template, null, rawResponse);
    }

    protected virtual IReadOnlyList<AttributeModel> GetFilters(GeneratorSyntaxContext context,
        MethodDeclarationSyntax methodDeclarationSyntax,
        IReadOnlyList<AttributeModel> attributeModules,
        CancellationToken cancellationToken) {
        var filterList = new List<AttributeModel>();

        filterList.AddRange(
            attributeModules.Where(a => IsFilterAttribute(a.TypeDefinition.Name)));
        filterList.AddRange(GetFiltersForClass(context,
            methodDeclarationSyntax.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault(),
            cancellationToken));

        return filterList;
    }

    protected virtual IEnumerable<AttributeModel> GetFiltersForClass(
        GeneratorSyntaxContext context,
        ClassDeclarationSyntax? parent,
        CancellationToken cancellationToken) {
        if (parent == null) {
            return Enumerable.Empty<AttributeModel>();
        }

        return GetFiltersFromAttributes(context, parent.AttributeLists, cancellationToken);
    }

    protected virtual bool IsFilterAttribute(string attributeName) {

        if (attributeName.Contains("DependencyModule")) {
            return false;
        }

        if (attributeName.Contains(".Attribute")) {
            return false;
        }

        return !AttributeNames().Contains(attributeName);
    }

    protected abstract IEnumerable<string> AttributeNames();

    protected virtual IEnumerable<AttributeModel> GetFiltersForMethod(
        GeneratorSyntaxContext context,
        MethodDeclarationSyntax methodDeclarationSyntax,
        CancellationToken cancellationToken) {
        return GetFiltersFromAttributes(context, methodDeclarationSyntax.AttributeLists,
            cancellationToken);
    }

    protected virtual IEnumerable<AttributeModel> GetFiltersFromAttributes(
        GeneratorSyntaxContext context,
        SyntaxList<AttributeListSyntax> attributeListSyntax,
        CancellationToken cancellationToken) {

        return AttributeModelHelper.GetAttributes(
            context,
            attributeListSyntax,
            cancellationToken,
            a => IsFilterAttribute(a.Name.ToString()));
    }
}