namespace SimpleRequest.Models.Attributes;

public class GetAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Get, path);

public class PutAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Put, path);

public class PatchAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Patch, path);

public class PostAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Post, path);

public class DeleteAttribute(string path = "/") : HttpAttribute(RequestConstants.HttpVerbs.Delete, path);

public class HttpAttribute(string verb, string path) : BaseFunctionAttribute {
    public string Verb {
        get;
    } = verb;

    public string Path {
        get;
    } = path;
}