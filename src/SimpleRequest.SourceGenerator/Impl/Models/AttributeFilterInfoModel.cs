using CSharpAuthor;
using DependencyModules.SourceGenerator.Impl.Models;

namespace SimpleRequest.SourceGenerator.Impl.Models;

public enum RequestFilterAttributeLifeCycle {
    /// <summary>
    /// New instance will be created for each use
    /// </summary>
    Transient,
    /// <summary>
    /// One instance of the filter will be used per attributed method
    /// </summary>
    Reuse,
}

public record AttributeFilterInfoModel(
    ITypeDefinition FilterType,
    ConstructorInfoModel? ConstructorInfo,
    RequestFilterAttributeLifeCycle LifeCycle,
    int Order,
    ServiceModel? ServiceModel,
    IReadOnlyList<ParameterInfoModel> Parameters,
    IReadOnlyList<PropertyInfoModel> Properties);

public class AttributeFilterInfoModelComparer : IEqualityComparer<AttributeFilterInfoModel> {
    private ServiceModelComparer _serviceModelComparer = new ServiceModelComparer();

    public bool Equals(AttributeFilterInfoModel? x, AttributeFilterInfoModel? y) {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        if (!_serviceModelComparer.Equals(x.ServiceModel, y.ServiceModel)) return false;
        return x.FilterType.Equals(y.FilterType) && x.LifeCycle == y.LifeCycle && x.Order == y.Order;
    }

    public int GetHashCode(AttributeFilterInfoModel obj) {
        unchecked {
            return (obj.FilterType.GetHashCode() * 397) ^
                   (int)obj.LifeCycle ^
                   obj.Order ^
                   (obj.ServiceModel != null ? _serviceModelComparer.GetHashCode(obj.ServiceModel) : 13);
        }
    }
}