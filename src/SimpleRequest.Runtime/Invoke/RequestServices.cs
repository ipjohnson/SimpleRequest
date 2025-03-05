using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Serializers;

namespace SimpleRequest.Runtime.Invoke;

[SingletonService]
public class RequestServices(
    IContentSerializerManager contentSerializerManager,
    IJsonSerializer jsonSerializer,
    IParameterBindingService bindingService) {
    public IContentSerializerManager ContentSerializerManager {
        get;
    } = contentSerializerManager;

    public IJsonSerializer JsonSerializer {
        get;
    } = jsonSerializer;

    public IParameterBindingService BindingService {
        get;
    } = bindingService;
}