using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Swagger.Services;
using SimpleRequest.Web.Runtime.Attributes;

namespace SimpleRequest.Swagger.Handlers;

public class IndexHandler(IOpenApiDocumentGenerator generator) {

    [Get("/swagger/v1/swagger.json")]
    public async Task GetDocument(IRequestContext context) {
        var document = await generator.GenerateDocument();

        var textWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(textWriter);
        
        document.SerializeAsV2(jsonWriter);

        if (context.ResponseData.Body != null) {
            context.ResponseData.ContentType = "application/json";
            await context.ResponseData.Body.WriteAsync(
                Encoding.UTF8.GetBytes(textWriter.ToString()));
        }
    }
}