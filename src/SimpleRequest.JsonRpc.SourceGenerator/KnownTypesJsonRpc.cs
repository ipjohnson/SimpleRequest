using CSharpAuthor;

namespace SimpleRequest.JsonRpc.SourceGenerator;

public class KnownTypesJsonRpc {
    
    public static ITypeDefinition JsonRpcFunctionAttribute = 
        TypeDefinition.Get("SimpleRequest.JsonRpc", "JsonRpcFunctionAttribute");
    
    public static ITypeDefinition JsonRpcServiceAttribute = 
        TypeDefinition.Get("SimpleRequest.JsonRpc", "JsonRpcServiceAttribute");
    
    public static ITypeDefinition JsonRpcRoutingHandlerInfo = 
        TypeDefinition.Get("SimpleRequest.JsonRpc.Impl", "JsonRpcRoutingHandlerInfo");
}