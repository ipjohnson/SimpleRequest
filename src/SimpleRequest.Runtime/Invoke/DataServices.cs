using DependencyModules.Runtime.Attributes;
using SimpleRequest.Runtime.Serializers;
using SimpleRequest.Runtime.Serializers.Json;
using SimpleRequest.Runtime.Templates;

namespace SimpleRequest.Runtime.Invoke;

[SingletonService]
public class DataServices(
    ITemplateInvocationEngine templateInvocationEngine,
    IContentSerializerManager contentSerializerManager,
    IJsonSerializer jsonSerializer,
    IParameterBindingService bindingService) {

    public ITemplateInvocationEngine TemplateInvocationEngine {
        get;
    } = templateInvocationEngine;

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