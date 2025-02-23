using CSharpAuthor;

namespace SimpleRequest.Web.SourceGenerator;

public class KnownWebTypes {
    public static readonly ITypeDefinition SimpleRequestWebModuleAttribute = 
        TypeDefinition.Get("SimpleRequest.Web.Runtime", "SimpleRequestWeb.Attribute");
    
    public static class Attributes {
        public const string Namespace = "SimpleRequest.Web.Runtime.Attributes";
        
        public static readonly ITypeDefinition Delete = TypeDefinition.Get(Namespace, "DeleteAttribute");
        
        public static readonly ITypeDefinition Get = TypeDefinition.Get(Namespace, "GetAttribute");
        
        public static readonly ITypeDefinition Patch = TypeDefinition.Get(Namespace, "PatchAttribute");
        
        public static readonly ITypeDefinition Post = TypeDefinition.Get(Namespace, "PostAttribute");
        
        public static readonly ITypeDefinition Put = TypeDefinition.Get(Namespace, "PutAttribute");
    }
}