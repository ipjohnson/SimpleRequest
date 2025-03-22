using SimpleRequest.Web.AspNetHost;
using TestApp.AspNet;

[assembly: TestApp.WebHandlers.TestWebHandlers]
[assembly: SimpleRequest.Swagger.SimpleRequestSwagger]
[assembly: SimpleRequest.SwaggerUi.SimpleRequestSwaggerUi]

AspNetWebHost.Run<ApplicationModule>();