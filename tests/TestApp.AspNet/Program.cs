using DependencyModules.Runtime;
using SimpleRequest.Web.AspNetHost;
using TestApp.AspNet;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddModule<TestWebApp>();
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseSimpleRequest();

//app.Use(new HttpHandler().Invoke);
app.Run();
