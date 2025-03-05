using DependencyModules.xUnit.Impl;
using SimpleRequest.Runtime.Invoke;

namespace SimpleRequest.Testing.Interfaces;

public interface IRequestDataEnrichment {
    IRequestData EnrichRequestData(
        IServiceProvider serviceProvider, 
        ITestCaseInfo testCaseInfo,
        IRequestData requestData);
}