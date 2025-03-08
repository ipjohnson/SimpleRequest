namespace SimpleRequest.Caching;

public interface ICacheProfile : ICacheConfiguration{
    string Name { get; }

}

public interface ICacheConfiguration {
    double Timeout { get; }

    string[]? VaryByHeader { get; }

    string[]? VaryByQuery { get; }

    string[]? PreserveHeader { get; }

    ResponseCacheLocation Location { get; }

    bool NoStore { get; }
}

public abstract class CacheProfile : ICacheProfile {

    public abstract string Name { get; }

    public virtual double Timeout { get; } = 60;

    public virtual string[]? VaryByHeader { get; } = Array.Empty<string>();

    public virtual string[]? VaryByQuery { get; } = Array.Empty<string>();

    public virtual string[]? PreserveHeader { get; } = Array.Empty<string>();

    public ResponseCacheLocation Location { get; } = ResponseCacheLocation.Any;

    public bool NoStore { get; } = false;
}