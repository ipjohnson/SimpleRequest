using CSharpAuthor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SimpleRequest.RazorBlade.SourceGenerator.Models;

namespace SimpleRequest.RazorBlade.SourceGenerator.Impl;

public class CshtmlFileModelConverter {

    public CshtmlFileModel ToTemplateModel(
        (AdditionalText Left, AnalyzerConfigOptionsProvider Right) pair, CancellationToken cancellation) {
        var (modelName, namespaces) = ParseUsingAndModel(pair.Left.GetText()?.ToString() ?? "", cancellation);
        
        return new CshtmlFileModel(
            pair.Left.Path,
            modelName,
            namespaces);
    }

    private (string modelName, List<string> namespaces) ParseUsingAndModel(string toString, CancellationToken cancellation) {
        var namespaces = new List<string>();
        var modelName = string.Empty;

        var lines = toString.Split('\n');

        foreach (var line in lines) {
            cancellation.ThrowIfCancellationRequested();
            if (line.StartsWith("@using ")) {
                var stringValues = line.Split(' ');
                var namespaceValue = stringValues.Last().TrimEnd(';');
                namespaces.Add(namespaceValue);
            } else if (line.StartsWith("@inherits ")) {
                var lessThanIndex = line.IndexOf('<');

                if (lessThanIndex > -1) {
                    var commaIndex = line.IndexOf(',', lessThanIndex + 1);

                    if (commaIndex > -1) {
                        modelName = line.Substring(
                            lessThanIndex + 1,commaIndex - lessThanIndex -1);
                    }
                    else {
                        var endIndex = line.IndexOf('>');
                        modelName = line.Substring(
                            lessThanIndex + 1,endIndex - lessThanIndex - 1);
                    }
                }
            }
        }
        
        return (modelName, namespaces);
    }
}