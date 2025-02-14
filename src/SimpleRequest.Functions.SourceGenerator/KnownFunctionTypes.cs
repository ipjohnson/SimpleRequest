using CSharpAuthor;

namespace SimpleRequest.Functions.SourceGenerator;

public static class KnownFunctionTypes {
    public const string FunctionNamespace = "SimpleRequest.Functions.Runtime";
    public const string FunctionAttributeNamespace = "SimpleRequest.Functions.Runtime.Attributes";

    public static readonly ITypeDefinition FunctionAttribute =
        TypeDefinition.Get(FunctionAttributeNamespace, "FunctionAttribute");

    public static readonly ITypeDefinition SimpleRequestFunctionsModuleAttribute =
        TypeDefinition.Get(FunctionNamespace, "SimpleRequestFunctions.Attribute");

    
}