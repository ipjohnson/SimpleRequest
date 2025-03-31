using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;
using static CSharpAuthor.SyntaxHelpers;

namespace SimpleRequest.SourceGenerator.Impl.Utils;

public class AttributeArrayHelper {
    public static IOutputComponent CreateAttributeArray(IReadOnlyList<AttributeModel> attributes) {
        var attributeStatements = new List<object>();

        foreach (var attributeModel in attributes) {
            var newStatement = New(attributeModel.TypeDefinition,
                attributeModel.GetArguments().OfType<object>().ToArray());

            if (attributeModel.Properties.Count > 0) {
                foreach (var propertyValue in attributeModel.PropertyValues()) {
                    
                    newStatement.AddInitValue(propertyValue);
                }
            }
            
            attributeStatements.Add(newStatement);
        }

        if (attributeStatements.Count > 0) {
            return NewArray(typeof(Attribute), attributeStatements.ToArray());
        }
        
        return CodeOutputComponent.Get("Array.Empty<Attribute>()");
    }
}