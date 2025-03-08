using System.Reflection;
using System.Text;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.SwaggerUi.Services;

public interface ISwaggerIndexTransformer {
    Task<byte[]> Transform(byte[] bytes);
}

[SingletonService]
public class SwaggerIndexTransformer : ISwaggerIndexTransformer {
    public Task<byte[]> Transform(byte[] bytes) {
        var indexString = Encoding.UTF8.GetString(bytes);
        var outputString = indexString;
        
        var titleIndex = indexString.IndexOf("<title>", StringComparison.OrdinalIgnoreCase);
        var titleEndIndex = indexString.IndexOf("</title>", StringComparison.OrdinalIgnoreCase);
        
        if (titleIndex > -1) {
            outputString = indexString.Substring(0, titleIndex + 7);
            outputString += Assembly.GetEntryAssembly()?.GetName().Name ?? "Swagger";
            outputString += indexString.Substring(titleEndIndex);
        }
        
        return Task.FromResult(Encoding.UTF8.GetBytes(outputString));
    }
}