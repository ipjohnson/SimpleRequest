using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;

namespace SimpleRequest.SourceGenerator.Impl.Models;

public record RequestHandlerNameModel(string Path, string Method);

public record ResponseInformationModel(
    bool IsAsync,
    ITypeDefinition? ReturnType,
    string? TemplateName,
    int? DefaultStatusCode,
    string? RawResponseContentType);

public record RequestHandlerModel(
    RequestHandlerNameModel Name,
    ITypeDefinition HandlerType,
    string HandlerMethod,
    ConstructorInfoModel? ConstructorInfo,
    ITypeDefinition GenerateInvokeType,
    IReadOnlyList<RequestParameterInformation> RequestParameterInformationList,
    ResponseInformationModel ResponseInformation,
    IReadOnlyList<AttributeModel> Filters
);

public class RequestHandlerModelComparer : IEqualityComparer<RequestHandlerModel> {
    public bool Equals(RequestHandlerModel? x, RequestHandlerModel? y) {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.Name.Equals(y.Name) && 
               x.HandlerType.Equals(y.HandlerType) && 
               x.HandlerMethod == y.HandlerMethod && 
               x.ConstructorInfo.Equals(y.ConstructorInfo) &&
               x.GenerateInvokeType.Equals(y.GenerateInvokeType) && 
               x.RequestParameterInformationList.Equals(y.RequestParameterInformationList) && 
               x.ResponseInformation.Equals(y.ResponseInformation) && 
               x.Filters.Equals(y.Filters);
    }

    public int GetHashCode(RequestHandlerModel obj) {
        unchecked {
            var hashCode = obj.Name.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.HandlerType.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.HandlerMethod.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.GenerateInvokeType.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.RequestParameterInformationList.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.ResponseInformation.GetHashCode();
            hashCode = (hashCode * 397) ^ obj.Filters.GetHashCode();
            return hashCode;
        }
    }
}