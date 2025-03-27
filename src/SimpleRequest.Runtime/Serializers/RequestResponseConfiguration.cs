namespace SimpleRequest.Runtime.Serializers;

public class RequestResponseConfiguration {
    /// <summary>
    /// Requests that return a plain string will use this content type
    /// to locate a serializer.
    /// </summary>
    public string DefaultStingContentType { get; set; } = "text/plain";
    
    /// <summary>
    /// Requests that provide Accept-Encoding: gzip, will compress by default
    /// set to false to disable behavior
    /// </summary>
    public bool EnableCompressionSupport { get; set; } = true;
}

public interface IRequestResponseConfigurationProvider {
    void Configure(RequestResponseConfiguration configuration);
}