namespace SimpleRequest.Models.Attributes;

public class GetAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Get, path);

public class PutAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Put, path);

public class PatchAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Patch, path);

public class PostAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Post, path);

public class DeleteAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Delete, path);


public class HeadAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Head, path);

public interface IHttpAttribute {
    string Verb { get; }
    string Path { get; }
    
    HttpMethod HttpMethod { get; }
}

public class HttpAttribute(string verb, string path) : BaseFunctionAttribute, IHttpAttribute {
    public string Verb {
        get;
    } = verb;

    public string Path {
        get;
    } = path;

    HttpMethod IHttpAttribute.HttpMethod { get; } = HttpMethod.Parse(verb);
}