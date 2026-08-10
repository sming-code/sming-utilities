using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace SmingCode.Utilities.DelegateInvokers.Tests;
using Testing.AutoDomainData;

public class DelegateInvoker_1_Tests
{
    [Theory]
    [AutoDomainData]
    public async Task Test1(int intToPass)
    {
        // Arrange
        var sut = DelegateFuncInvoker<IServiceProvider, int, string>.FromDelegate(
            async (
                [FromServices] ITestClass testClass,
                int testInt
            ) =>
            {
                return await Task.FromResult($"Result_{testClass.TestProp}_{testInt}");
            },
            new TestDelegateParameterBuilderBuilder()
        );
        var serviceProvider = new ServiceCollection()
            .AddScoped<ITestClass, TestClass>()
            .BuildServiceProvider();
        var expectedResult = $"Result_TestPropValue_{intToPass}";

        // Act
        var result = await sut.Invoke(serviceProvider, intToPass);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [AutoDomainData]
    public async Task Test2(
        string inputString,
        string stringToAppend
    )
    {
        // Arrange
        var sut = DelegateActionInvoker<string>.FromDelegate(
            async (
                string testString
            ) =>
            {
                inputString += testString;
            },
            new TestDelegateParameterBuilderBuilder2()
        );
        var serviceProvider = new ServiceCollection()
            .AddScoped<ITestClass, TestClass>()
            .BuildServiceProvider();
        var expectedResult = $"{inputString}{stringToAppend}";

        // Act
        await sut.Invoke(stringToAppend);

        // Assert
        inputString.ShouldBe(expectedResult);
    }
}

internal class TestClass2(
    Delegate testDelegate
)
{
    internal Delegate TestDelegate { get; } = testDelegate;
}

[AttributeUsage(AttributeTargets.Parameter)]
internal class FromServicesAttribute : Attribute
{ }

internal interface ITestClass
{
    public string TestProp { get; }
}

internal class TestClass : ITestClass
{
    public string TestProp => "TestPropValue";
}

internal class TestDelegateParameterBuilderBuilder : DelegateParameterBuilderBuilder<IServiceProvider, int>
{
    public override Func<IServiceProvider, int, TParam> BuildParameterBuilder<TParam>(ParameterInfo parameterInfo)
        => parameterInfo.GetCustomAttribute<FromServicesAttribute>() is not null
            ? (serviceProvider, _) => serviceProvider.GetService<TParam>()!
            : (_, valuePassed) => (TParam)(object)valuePassed!;
}

internal class TestDelegateParameterBuilderBuilder2 : DelegateParameterBuilderBuilder<string>
{
    public override Func<string, TParam> BuildParameterBuilder<TParam>(ParameterInfo parameterInfo)
        => (valuePassed) => (TParam)(object)valuePassed!;
}