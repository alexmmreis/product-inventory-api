using FluentAssertions;
using Reqnroll;

namespace ProductInventory.Bdd.Tests.Steps;

/// <summary>Step shared by every feature that asserts on the last HTTP response's status code.</summary>
[Binding]
public class ApiResponseSteps
{
    private readonly ScenarioContext _scenarioContext;

    public ApiResponseSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Then(@"the response status should be (\d+)")]
    public void ThenTheResponseStatusShouldBe(int expectedStatusCode)
    {
        var response = _scenarioContext.Get<HttpResponseMessage>("Response");
        ((int)response.StatusCode).Should().Be(expectedStatusCode);
    }
}
