namespace SimpleRequest.Models;

public enum BindingType {
    Body,
    Query,
    Path,
    Header,
    FromServices,
    RequestData,
    ResponseData,
    Context,
    CustomAttribute
}