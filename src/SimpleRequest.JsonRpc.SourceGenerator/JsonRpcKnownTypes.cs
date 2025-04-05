using CSharpAuthor;

namespace SimpleRequest.JsonRpc.SourceGenerator;

public class JsonRpcKnownTypes {
    
    public static ITypeDefinition JsonRpcFunctionAttribute = 
        TypeDefinition.Get("SimpleRequest.JsonRpc", "JsonRpcFunctionAttribute");
    
    public static ITypeDefinition JsonRpcServiceAttribute = 
        TypeDefinition.Get("SimpleRequest.JsonRpc", "JsonRpcServiceAttribute");
}