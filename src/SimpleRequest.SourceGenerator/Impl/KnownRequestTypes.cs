using CSharpAuthor;

namespace SimpleRequest.SourceGenerator.Impl;

// ReSharper disable InconsistentNaming
public static class KnownRequestTypes {
    public const string InvokeNamespace = "SimpleRequest.Runtime.Invoke";
    public const string SerializersNamespace = "SimpleRequest.Runtime.Serializers";
    public const string AttributeNamespace = "SimpleRequest.Runtime.Attributes";

    
    public static readonly ITypeDefinition RequestFilterAttribute =
        TypeDefinition.Get(
            TypeDefinitionEnum.ClassDefinition, 
            AttributeNamespace, 
            "RequestFilterAttribute");
    
    
    public static readonly ITypeDefinition ISimpleRequestEntryAttribute =
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            AttributeNamespace, 
            "ISimpleRequestEntryAttribute");
    
    public static readonly ITypeDefinition IRequestContext =
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IRequestContext");

    public static readonly ITypeDefinition IRequestData = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IRequestData");
    
    public static readonly ITypeDefinition IResponseData = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IResponseData");
    
    public static readonly ITypeDefinition IInvokeParameters = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IInvokeParameters");
    
    public static readonly ITypeDefinition IContentSerializerManager =
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            SerializersNamespace , 
            "IContentSerializerManager");
    
    public static readonly ITypeDefinition IInvokeParameterInfo = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IInvokeParameterInfo");
    
    public static readonly ITypeDefinition IRequestHandlerInfoMethods = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IRequestHandlerInfoMethods");
    
    public static readonly ITypeDefinition IRequestHandlerProvider = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IRequestHandlerProvider");
    
    public static readonly ITypeDefinition RequestHandlerInfoMethods = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace + ".Impl", 
            "RequestHandlerInfoMethods");
    
    public static readonly ITypeDefinition IRequestHandlerInfo = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IRequestHandlerInfo");
    
    public static readonly ITypeDefinition IRequestHandler = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IRequestHandler");
    
    public static readonly ITypeDefinition IRequestHandlerFactory = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IRequestHandlerFactory");
    
    public static readonly ITypeDefinition IRequestFilterProvider = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "IRequestFilterProvider");
    
    public static readonly ITypeDefinition RequestHandlerInfo = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace + ".Impl", 
            "RequestHandlerInfo");
    
    public static readonly ITypeDefinition InvokeParameterInfo = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace + ".Impl", 
            "InvokeParameterInfo");

    public static readonly ITypeDefinition  RequestFilterInfo  = 
        TypeDefinition.Get(
            TypeDefinitionEnum.InterfaceDefinition, 
            InvokeNamespace, 
            "RequestFilterInfo");
}