using System.Text;
using Microsoft.OpenApi.Writers;
using SimpleRequest.Caching;
using SimpleRequest.Models.Attributes;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Swagger.Services;

namespace SimpleRequest.Swagger.Handlers;

public class IndexHandler(IOpenApiDocumentGenerator generator) {

    [Get("/swagger/v1/swagger.json")]
    [ResponseCache]
    public async Task GetDocument(IRequestContext context) {
        var document = await generator.GenerateDocument();

        var textWriter = new StringWriter();
        var jsonWriter = new OpenApiJsonWriter(textWriter);
        
        document.SerializeAsV2(jsonWriter);

        if (context.ResponseData.Body != null) {
            context.ResponseData.Status = 200;
            context.ResponseData.ContentType = "application/json";
            await context.ResponseData.Body.WriteAsync(
                Encoding.UTF8.GetBytes(textWriter.ToString()));
        }
    }
}