namespace SimpleRequest.Models.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public class TagsAttribute(params string[] tags) : Attribute {
    public string[] Tags {
        get;
    } = tags;

}