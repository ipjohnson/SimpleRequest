using System.Xml.Linq;

namespace SimpleRequest.Swagger.Services;


public static class XmlDocumentationExtensions
{
    public static string? GetSummary(this XElement element)
    {
        return element?.Descendants("summary").FirstOrDefault()?.Value.Trim();
    }

    public static string? GetExample(this XElement element)
    {
        return element?.Descendants("example").FirstOrDefault()?.Value.Trim();
    }

    public static IEnumerable<string> GetTags(this XElement element)
    {
        return element?.Descendants("tag").Select(e => e.Value.Trim()) ?? Array.Empty<string>();
    }

    public static string? GetParameterSummary(this XElement element, string parameterName)
    {
        return element?.Descendants("param").FirstOrDefault(e => e.Attribute("name")?.Value == parameterName)?.Value.Trim();
    }
}