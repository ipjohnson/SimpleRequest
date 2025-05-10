using CSharpAuthor;

namespace SimpleRequest.SourceGenerator.Impl;

public class OutputContextDefault {
    public static OutputContextOptions Instance { get; } = new OutputContextOptions {
        GenerateDocumentation = true,
        BreakInvokeLines = true,
        TypeOutputMode = TypeOutputMode.Global
    };
}