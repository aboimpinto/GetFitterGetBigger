using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Fixtures;
using GetFitterGetBigger.API.Models.Entities;
using GetFitterGetBigger.API.Models.SpecializedIds;
using Microsoft.EntityFrameworkCore;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.ReferenceData;

[Binding]
public class ExecutionProtocolsSteps
{
    private readonly ScenarioContext _scenarioContext;
    private readonly PostgreSqlTestFixture _fixture;
    private ExecutionProtocolsResponseDto? _executionProtocolsResponse;
    private ExecutionProtocolDto? _executionProtocolResult;

    public ExecutionProtocolsSteps(ScenarioContext scenarioContext, PostgreSqlTestFixture fixture)
    {
        _scenarioContext = scenarioContext;
        _fixture = fixture;
    }

    [Given(@"the following execution protocols exist in the database:")]
    public async Task GivenTheFollowingExecutionProtocolsExistInTheDatabase(Table table)
    {
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            // Clear existing execution protocols
            var existingProtocols = await context.ExecutionProtocols.ToListAsync();
            context.ExecutionProtocols.RemoveRange(existingProtocols);
            await context.SaveChangesAsync();

            // Add new execution protocols
            foreach (var row in table.Rows)
            {
                var protocol = ExecutionProtocol.Handler.Create(
                    ExecutionProtocolId.From(row["ExecutionProtocolId"]),
                    row["Value"],
                    row.ContainsKey("Description") ? row["Description"] : null,
                    row["Code"],
                    bool.Parse(row["TimeBase"]),
                    bool.Parse(row["RepBase"]),
                    row.ContainsKey("RestPattern") ? row["RestPattern"] : null,
                    row.ContainsKey("IntensityLevel") ? row["IntensityLevel"] : null,
                    int.Parse(row["DisplayOrder"]),
                    bool.Parse(row["IsActive"])
                );
                
                context.ExecutionProtocols.Add(protocol);
            }
            
            await context.SaveChangesAsync();
        });
    }

    [Given(@"an execution protocol with code ""(.*)"" exists")]
    public async Task GivenAnExecutionProtocolWithCodeExists(string code)
    {
        // This step assumes the protocol was already created in the Background section
        await _fixture.ExecuteDbContextAsync(async context =>
        {
            var protocol = await context.ExecutionProtocols
                .FirstOrDefaultAsync(p => p.Code == code);
            
            protocol.Should().NotBeNull($"Expected execution protocol with code '{code}' to exist");
        });
    }

    [Then(@"the response should contain (\d+) execution protocols")]
    public async Task ThenTheResponseShouldContainExecutionProtocols(int expectedCount)
    {
        var response = _scenarioContext.GetLastResponse();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _executionProtocolsResponse = await response.Content.ReadFromJsonAsync<ExecutionProtocolsResponseDto>();
        _executionProtocolsResponse.Should().NotBeNull();
        _executionProtocolsResponse!.ExecutionProtocols.Should().HaveCount(expectedCount);
    }

    [Then(@"each execution protocol should have the following fields:")]
    public void ThenEachExecutionProtocolShouldHaveTheFollowingFields(Table table)
    {
        _executionProtocolsResponse.Should().NotBeNull();
        
        foreach (var protocol in _executionProtocolsResponse!.ExecutionProtocols)
        {
            foreach (var row in table.Rows)
            {
                var fieldName = row["Field"];
                var fieldType = row["Type"];
                var isRequired = bool.Parse(row["Required"]);
                
                var property = typeof(ExecutionProtocolDto).GetProperty(fieldName);
                property.Should().NotBeNull($"Field {fieldName} should exist");
                
                if (isRequired)
                {
                    var value = property!.GetValue(protocol);
                    value.Should().NotBeNull($"Required field {fieldName} should not be null");
                }
                
                // Verify field type
                var actualType = property!.PropertyType;
                switch (fieldType.ToLower())
                {
                    case "string":
                        actualType.Should().Be(typeof(string));
                        break;
                    case "boolean":
                        actualType.Should().Be(typeof(bool));
                        break;
                    case "number":
                        actualType.Should().Be(typeof(int));
                        break;
                }
            }
        }
    }

    [Then(@"the execution protocols should be ordered by displayOrder ascending")]
    public void ThenTheExecutionProtocolsShouldBeOrderedByDisplayOrderAscending()
    {
        _executionProtocolsResponse.Should().NotBeNull();
        _executionProtocolsResponse!.ExecutionProtocols
            .Should().BeInAscendingOrder(x => x.DisplayOrder);
    }

    [Then(@"no inactive protocols should be included")]
    public void ThenNoInactiveProtocolsShouldBeIncluded()
    {
        _executionProtocolsResponse.Should().NotBeNull();
        _executionProtocolsResponse!.ExecutionProtocols
            .Should().OnlyContain(x => x.IsActive == true);
    }

    [Then(@"the response should include both active and inactive protocols")]
    public void ThenTheResponseShouldIncludeBothActiveAndInactiveProtocols()
    {
        _executionProtocolsResponse.Should().NotBeNull();
        _executionProtocolsResponse!.ExecutionProtocols
            .Should().Contain(x => x.IsActive == true)
            .And.Contain(x => x.IsActive == false);
    }

    [Then(@"the response should contain an execution protocol with:")]
    public async Task ThenTheResponseShouldContainAnExecutionProtocolWith(Table table)
    {
        var response = _scenarioContext.GetLastResponse();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _executionProtocolResult = await response.Content.ReadFromJsonAsync<ExecutionProtocolDto>();
        _executionProtocolResult.Should().NotBeNull();
        
        foreach (var row in table.Rows)
        {
            var fieldName = row["Field"];
            var expectedValue = row["Value"];
            
            var property = typeof(ExecutionProtocolDto).GetProperty(fieldName);
            property.Should().NotBeNull($"Field {fieldName} should exist");
            
            var actualValue = property!.GetValue(_executionProtocolResult)?.ToString();
            actualValue.Should().Be(expectedValue, $"Field {fieldName} should have value {expectedValue}");
        }
    }

    [Then(@"at least one protocol should have both timeBase and repBase as true")]
    public void ThenAtLeastOneProtocolShouldHaveBothTimeBaseAndRepBaseAsTrue()
    {
        _executionProtocolsResponse.Should().NotBeNull();
        _executionProtocolsResponse!.ExecutionProtocols
            .Should().Contain(x => x.TimeBase == true && x.RepBase == true);
    }

    [Then(@"at least one protocol should have timeBase true and repBase false")]
    public void ThenAtLeastOneProtocolShouldHaveTimeBaseTrueAndRepBaseFalse()
    {
        _executionProtocolsResponse.Should().NotBeNull();
        _executionProtocolsResponse!.ExecutionProtocols
            .Should().Contain(x => x.TimeBase == true && x.RepBase == false);
    }

    [Then(@"at least one protocol should have timeBase false and repBase true")]
    public void ThenAtLeastOneProtocolShouldHaveTimeBaseFalseAndRepBaseTrue()
    {
        _executionProtocolsResponse.Should().NotBeNull();
        _executionProtocolsResponse!.ExecutionProtocols
            .Should().Contain(x => x.TimeBase == false && x.RepBase == true);
    }
}