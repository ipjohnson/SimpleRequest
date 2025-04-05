namespace SimpleRequest.JsonRpc.Models;

public class JsonRpcErrorModel {
    public int Code { get; set; }
    
    public string Message { get; set; } = string.Empty;
    
    public object? Data { get; set; }
}

public class JsonRpcResponseModel {
    public string JsonRpc { get; } = "2.0";
    
    public object? Result { get; set; }
    
    public JsonRpcErrorModel? Error { get; set; }
    
    public object? Id { get; set; }
}