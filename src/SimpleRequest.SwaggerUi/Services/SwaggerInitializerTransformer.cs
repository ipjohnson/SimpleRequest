using System.Text;
using DependencyModules.Runtime.Attributes;

namespace SimpleRequest.SwaggerUi.Services;

public interface ISwaggerInitializerTransformer {
    Task<byte[]> Transform(byte[] data);
}

[SingletonService]
public class SwaggerInitializerTransformer : ISwaggerInitializerTransformer {

    public Task<byte[]> Transform(byte[] data) {
        var initializerFile = Encoding.UTF8.GetString(data);
        var outputString = initializerFile;
        
        var urlIndex = initializerFile.IndexOf("url:", StringComparison.InvariantCultureIgnoreCase);

        if (urlIndex > -1) {
            var commaIndex = initializerFile.IndexOf(",", urlIndex, StringComparison.InvariantCultureIgnoreCase);

            if (commaIndex != -1) {
                outputString = initializerFile.Substring(0, urlIndex + "url:".Length);
                outputString += " \"/swagger/v1/swagger.json\"";
                outputString += initializerFile.Substring(commaIndex);
            }
        }
        
        return Task.FromResult(Encoding.UTF8.GetBytes(outputString));
    }
}