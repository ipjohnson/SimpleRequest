using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;

namespace SimpleRequest.SourceGenerator.Impl.Models;

public enum ParameterBindType {
    Path,
    QueryString,
    Header,
    Cookie,
    Body,
    ServiceProvider,
    FromServiceProvider,
    RequestContext,
    RequestData,
    ResponseData,
    CustomAttribute,
}

public record RequestParameterInformation(
    ITypeDefinition ParameterType,
    string Name,
    bool Required,
    string? DefaultValue,
    ParameterBindType BindingType,
    string BindingName,
    int ParameterIndex,
    IReadOnlyList<AttributeModel> CustomAttributes);