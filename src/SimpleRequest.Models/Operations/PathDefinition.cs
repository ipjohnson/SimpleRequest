namespace SimpleRequest.Models.Operations;

public enum PathPartType {
    Constant,
    Path,
    QueryString
}

public record PathPart(string Value, PathPartType PartType, IOperationParameterInfo? ParameterInfo);

public record PathDefinition(string Template, IReadOnlyList<PathPart> Parts);