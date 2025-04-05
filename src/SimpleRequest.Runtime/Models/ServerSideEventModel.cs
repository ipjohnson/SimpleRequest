namespace SimpleRequest.Runtime.Models;

public record ServerSideEventModel(
    object Data, 
    string? EventName = null, 
    string? Id = null,
    int? Retry = null);