namespace SimpleRequest.Web.Runtime.Attributes;

public class DeleteAttribute(string path = "/") : WebRouteAttribute(path);

public class GetAttribute(string path = "/") : WebRouteAttribute(path) {
    public int NullReturn { get; set; } = 404;
}

public class PatchAttribute(string path = "/") : WebRouteAttribute(path);

public class PostAttribute(string path = "/") : WebRouteAttribute(path);

public class PutAttribute(string path = "/") : WebRouteAttribute(path);


public class WebRouteAttribute(string path) : Attribute {
    public string Path {
        get;
    } = path;

    public int SuccessStatus { get; set; } = 200;

    public int ValidationErrorStatus { get; set; } = 400;

    public int ErrorStatus { get; set; } = 500;
}