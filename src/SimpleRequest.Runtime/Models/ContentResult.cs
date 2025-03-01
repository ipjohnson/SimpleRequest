using System.Text;

namespace SimpleRequest.Runtime.Models;

public interface IContentResult {
    byte[] Content { get; }
    
    string ContentType { get; }
    int? StatusCode { get; }
    bool IsBinary { get; }
}

public class ContentResult : IContentResult {
    public static ContentResult Ok(string content, string contentType = "text/plain") {
        return new ContentResult(content, contentType);
    }
    
    public static ContentResult Ok(byte[] content, string contentType) {
        return new ContentResult(content, contentType);
    }
    
    public ContentResult(string content, string contentType, bool isBinary = false, int? statusCode = null) {
        Content = Encoding.UTF8.GetBytes(content);
        ContentType = contentType;
        IsBinary = isBinary;
        StatusCode = statusCode;
    }

    public ContentResult(byte[] content, string contentType,  bool isBinary = false,int? statusCode = null) {
        Content = content;
        ContentType = contentType;
        IsBinary = isBinary;
        StatusCode = statusCode;
    }
    
    public byte[] Content { get; }
    
    public string ContentType { get; }

    public bool IsBinary { get; }

    public int? StatusCode { get; }
}