using DependencyModules.xUnit.Impl;
using SimpleRequest.Runtime.Invoke;
using SimpleRequest.Testing.Interfaces;

namespace SimpleRequest.Testing.Attributes;

/// <summary>
/// Add header to all test requests
/// </summary>
/// <param name="name"></param>
/// <param name="value"></param>
public class RequestHeaderAttribute(string name, string value) : Attribute, IRequestDataEnrichment {
    
    public IRequestData EnrichRequestData(
        IServiceProvider serviceProvider,
        ITestCaseInfo testCaseInfo,
        IRequestData requestData) {
        
        requestData.Headers[name] = value;
        
        return requestData;
    }
}