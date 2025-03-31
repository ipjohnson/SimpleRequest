namespace SimpleRequest.JsonRpc;

public class JsonRpcFunctionAttribute(string name) : Attribute {
    public string Name {
        get;
    } = name;

    public string[] Tags { get; set; } = [];
}