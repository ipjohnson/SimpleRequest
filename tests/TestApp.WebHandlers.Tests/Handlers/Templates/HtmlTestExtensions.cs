using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using SimpleRequest.Testing;

namespace TestApp.WebHandlers.Tests.Handlers.Templates;

public static class HtmlTestExtensions {
    public static IHtmlDocument ParseHtml(this ResponseModel response) {
        var htmlParser = new HtmlParser();
        var streamReader = new StreamReader(response.Body);
        var htmlString = streamReader.ReadToEnd();
        
        var result = htmlParser.ParseDocument(htmlString);
        
        return result;
    }
}