using System.Text;
using DependencyModules.xUnit.Attributes;
using Microsoft.CodeAnalysis;
using NSubstitute;
using SimpleRequest.Client.Impl;
using SimpleRequest.Models.Operations;
using Xunit;

namespace SimpleRequest.Client.Tests.Impl;

public class PathBuilderTests {

    [ModuleTest]
    public void NoParameterPath(
        PathBuilder sut,
        StringBuilder stringBuilder,
        [Mock] IOperationInfo operation) {
        operation.Path.Returns(new PathDefinition("/path", []));

        var path = sut.BuildPath(OperationParameters.Empty, stringBuilder);

        Assert.Equal("/path", path);
    }

    [ModuleTest]
    public void PathWithSingleParameter(
        PathBuilder sut,
        StringBuilder stringBuilder,
        [Mock] IOperationInfo operation) {
        var parameterInfo = new OperationParameterInfo<int>(
            "id",
            0,
            "id",
            ParameterBindType.Path,
            null,
            true,
            []);

        operation.Path.Returns(new PathDefinition("/path/{id}", [
            new PathPart("/path/", PathPartType.Constant, null),
            new PathPart("id", PathPartType.Path, parameterInfo)
        ]));

        var parameters = new OperationParameters([parameterInfo]);
        parameters.Set(123,0);
        
        var path = sut.BuildPath(parameters, stringBuilder);

        Assert.Equal("/path/123", path);
    }
    
    [ModuleTest]
    public void PathWithTwoParameters(
        PathBuilder sut,
        StringBuilder stringBuilder,
        [Mock] IOperationInfo operation) {
        var stringParam = new OperationParameterInfo<string>(
            "stringId",
            0,
            "stringId",
            ParameterBindType.Path,
            null,
            true,
            []);

        var intParam = new OperationParameterInfo<int>(
            "id",
            1,
            "id",
            ParameterBindType.Path,
            null,
            true,
            []);
        
        operation.Path.Returns(new PathDefinition("/path/{stringId}/{id}", [
            new PathPart("/path/", PathPartType.Constant, null),
            new PathPart("stringId", PathPartType.Path, stringParam),
            new PathPart("/", PathPartType.Constant, null),
            new PathPart("id", PathPartType.Path, intParam),
        ]));

        var parameters = new OperationParameters([stringParam, intParam]);
        parameters.Set("category", 0);
        parameters.Set(456, 1);

        var path = sut.BuildPath(parameters, stringBuilder);

        Assert.Equal("/path/category/456", path);
    }
}