using CSharpAuthor;

namespace SimpleRequest.RazorBlade.SourceGenerator.Models;

public record CshtmlFileModel(
    string FilePath,
    string ModelName,
    IReadOnlyList<string> Namespaces);