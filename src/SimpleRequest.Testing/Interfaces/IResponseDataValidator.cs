using DependencyModules.xUnit.Impl;

namespace SimpleRequest.Testing.Interfaces;

public interface IResponseDataValidator {
    Task ValidateResponse(IServiceProvider serviceProvider,ITestCaseInfo testCaseInfo,  ResponseModel response);
}