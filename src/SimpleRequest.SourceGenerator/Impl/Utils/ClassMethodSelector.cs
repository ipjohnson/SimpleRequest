using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SimpleRequest.SourceGenerator.Impl.Utils;

public class ClassMethodSelector : SyntaxSelector<MethodDeclarationSyntax> {
    public ClassMethodSelector(params ITypeDefinition[] attributeTypes) : base(attributeTypes) {
        
    }
    
    protected override bool TestForTypes(SyntaxNode node, CancellationToken token) {
        if (base.TestForTypes(node, token)) {
            return node.Ancestors().OfType<ClassDeclarationSyntax>().Any();
        }
        
        return false;
    }
}