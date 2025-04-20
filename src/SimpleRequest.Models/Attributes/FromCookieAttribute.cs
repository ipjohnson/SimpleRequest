namespace SimpleRequest.Models.Attributes;

public class FromCookieAttribute : Attribute {
    public string Name { get; set; } = "";
}