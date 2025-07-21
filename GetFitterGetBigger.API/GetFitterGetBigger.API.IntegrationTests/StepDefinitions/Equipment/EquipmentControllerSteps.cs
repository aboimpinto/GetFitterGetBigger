using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using GetFitterGetBigger.API.DTOs;
using GetFitterGetBigger.API.IntegrationTests.TestInfrastructure.Helpers;
using TechTalk.SpecFlow;
using Xunit;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.Equipment;

[Binding]
public class EquipmentControllerSteps
{
    private readonly ScenarioContext _scenarioContext;
    private HttpResponseMessage? _lastResponse;
    private List<EquipmentDto>? _equipmentCollection;
    private EquipmentDto? _equipmentResult;
    private readonly List<string> _createdEquipmentIds = new();

    public EquipmentControllerSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"I have created equipment ""(.*)"" via API")]
    public async Task GivenIHaveCreatedEquipmentViaAPI(string equipmentName)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        var createDto = new CreateEquipmentDto
        {
            Name = equipmentName
        };

        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("/api/ReferenceTables/Equipment", content);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdEquipment = await response.Content.ReadFromJsonAsync<EquipmentDto>();
        createdEquipment.Should().NotBeNull();
        
        // Store for later use
        _createdEquipmentIds.Add(createdEquipment!.Id);
        _scenarioContext.Set(createdEquipment.Id, "CreatedEquipmentId");
        _scenarioContext.Set(equipmentName, "CreatedEquipmentName");
        
        // Store as "first" and "second" for multiple equipment scenarios
        if (!_scenarioContext.ContainsKey("FirstEquipmentId"))
        {
            _scenarioContext.Set(createdEquipment.Id, "FirstEquipmentId");
        }
        else if (!_scenarioContext.ContainsKey("SecondEquipmentId"))
        {
            _scenarioContext.Set(createdEquipment.Id, "SecondEquipmentId");
        }
    }

    [Given(@"I have created an exercise ""(.*)"" that uses the equipment")]
    public async Task GivenIHaveCreatedAnExerciseThatUsesTheEquipment(string exerciseName)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        var equipmentId = _scenarioContext.Get<string>("CreatedEquipmentId");
        
        // Get difficulty and kinetic chain IDs
        var difficultyResponse = await httpClient.GetAsync("/api/ReferenceTables/DifficultyLevels");
        var difficulties = await difficultyResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        var difficultyId = difficulties!.First().Id;
        
        var kineticResponse = await httpClient.GetAsync("/api/ReferenceTables/KineticChainTypes");
        var kineticChains = await kineticResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        var kineticChainId = kineticChains!.First().Id;
        
        // Get muscle groups and roles for exercise creation
        var muscleGroupsResponse = await httpClient.GetAsync("/api/ReferenceTables/MuscleGroups");
        var muscleGroups = await muscleGroupsResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        var muscleGroupId = muscleGroups!.First().Id;
        
        var muscleRolesResponse = await httpClient.GetAsync("/api/ReferenceTables/MuscleRoles");
        var muscleRoles = await muscleRolesResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        var primaryRoleId = muscleRoles!.First(r => r.Value == "Primary").Id;
        
        // Get weight type for exercise
        var weightTypesResponse = await httpClient.GetAsync("/api/ReferenceTables/ExerciseWeightTypes");
        var weightTypes = await weightTypesResponse.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        var weightTypeId = weightTypes!.First().Id;
        
        var createDto = new CreateExerciseRequest
        {
            Name = exerciseName,
            Description = "Test exercise",
            DifficultyId = difficultyId,
            KineticChainId = kineticChainId,
            EquipmentIds = new List<string> { equipmentId },
            IsUnilateral = false,
            ExerciseWeightTypeId = weightTypeId,
            MuscleGroups = new List<MuscleGroupWithRoleRequest>
            {
                new MuscleGroupWithRoleRequest
                {
                    MuscleGroupId = muscleGroupId,
                    MuscleRoleId = primaryRoleId
                }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("/api/Exercises", content);
        
        // Log error if creation fails
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Exercise creation failed with status {response.StatusCode}: {errorContent}");
            Console.WriteLine($"Request JSON: {JsonSerializer.Serialize(createDto, new JsonSerializerOptions { WriteIndented = true })}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [When(@"I get all equipment via API")]
    public async Task WhenIGetAllEquipmentViaAPI()
    {
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.GetAsync("/api/ReferenceTables/Equipment");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I get the created equipment by ID via API")]
    public async Task WhenIGetTheCreatedEquipmentByIdViaAPI()
    {
        var equipmentId = _scenarioContext.Get<string>("CreatedEquipmentId");
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.GetAsync($"/api/ReferenceTables/Equipment/{equipmentId}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I get equipment by ID ""(.*)"" via API")]
    public async Task WhenIGetEquipmentByIdViaAPI(string equipmentId)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.GetAsync($"/api/ReferenceTables/Equipment/{equipmentId}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I get equipment by name ""(.*)"" via API")]
    public async Task WhenIGetEquipmentByNameViaAPI(string equipmentName)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.GetAsync($"/api/ReferenceTables/Equipment/ByValue/{equipmentName}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I get equipment by value ""(.*)"" via API")]
    public async Task WhenIGetEquipmentByValueViaAPI(string equipmentValue)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.GetAsync($"/api/ReferenceTables/Equipment/ByValue/{equipmentValue}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I create equipment ""(.*)"" via API")]
    public async Task WhenICreateEquipmentViaAPI(string equipmentName)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        var createDto = new CreateEquipmentDto
        {
            Name = equipmentName
        };

        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        _lastResponse = await httpClient.PostAsync("/api/ReferenceTables/Equipment", content);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I update the equipment to name ""(.*)"" via API")]
    public async Task WhenIUpdateTheEquipmentToNameViaAPI(string newName)
    {
        var equipmentId = _scenarioContext.Get<string>("CreatedEquipmentId");
        var httpClient = _scenarioContext.GetHttpClient();
        
        var updateDto = new UpdateEquipmentDto
        {
            Name = newName
        };

        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        _lastResponse = await httpClient.PutAsync($"/api/ReferenceTables/Equipment/{equipmentId}", content);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I update equipment ""(.*)"" to name ""(.*)"" via API")]
    public async Task WhenIUpdateEquipmentToNameViaAPI(string equipmentId, string newName)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        var updateDto = new UpdateEquipmentDto
        {
            Name = newName
        };

        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        _lastResponse = await httpClient.PutAsync($"/api/ReferenceTables/Equipment/{equipmentId}", content);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I update the second equipment to name ""(.*)"" via API")]
    public async Task WhenIUpdateTheSecondEquipmentToNameViaAPI(string newName)
    {
        var equipmentId = _scenarioContext.Get<string>("SecondEquipmentId");
        var httpClient = _scenarioContext.GetHttpClient();
        
        var updateDto = new UpdateEquipmentDto
        {
            Name = newName
        };

        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        _lastResponse = await httpClient.PutAsync($"/api/ReferenceTables/Equipment/{equipmentId}", content);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I delete the equipment via API")]
    public async Task WhenIDeleteTheEquipmentViaAPI()
    {
        var equipmentId = _scenarioContext.Get<string>("CreatedEquipmentId");
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.DeleteAsync($"/api/ReferenceTables/Equipment/{equipmentId}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I delete equipment ""(.*)"" via API")]
    public async Task WhenIDeleteEquipmentViaAPI(string equipmentId)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.DeleteAsync($"/api/ReferenceTables/Equipment/{equipmentId}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [Then(@"I should receive a collection of equipment")]
    public async Task ThenIShouldReceiveACollectionOfEquipment()
    {
        _lastResponse.Should().NotBeNull();
        _equipmentCollection = await _lastResponse!.Content.ReadFromJsonAsync<List<EquipmentDto>>();
        _equipmentCollection.Should().NotBeNull();
    }

    [Then(@"all equipment items should be active")]
    public void ThenAllEquipmentItemsShouldBeActive()
    {
        _equipmentCollection.Should().NotBeNull();
        _equipmentCollection!.Should().NotBeEmpty();
        _equipmentCollection.Should().OnlyContain(e => e.IsActive);
    }

    [Then(@"I should receive the equipment")]
    public async Task ThenIShouldReceiveTheEquipment()
    {
        _lastResponse.Should().NotBeNull();
        // GetById returns EquipmentDto
        _equipmentResult = await _lastResponse!.Content.ReadFromJsonAsync<EquipmentDto>();
        _equipmentResult.Should().NotBeNull();
    }

    [Then(@"I should receive the equipment reference data")]
    public async Task ThenIShouldReceiveTheEquipmentReferenceData()
    {
        _lastResponse.Should().NotBeNull();
        _equipmentResult = await _lastResponse!.Content.ReadFromJsonAsync<EquipmentDto>();
        _equipmentResult.Should().NotBeNull();
    }

    [Then(@"the equipment name should be ""(.*)""")]
    public void ThenTheEquipmentNameShouldBe(string expectedName)
    {
        // Check which result object we have
        if (_equipmentResult != null)
        {
            // Get the response content for better error messages
            var responseContent = _scenarioContext.GetLastResponseContent();
            
            // If Name is null, provide detailed error
            if (_equipmentResult.Name == null)
            {
                Assert.Fail($"Equipment Name is null! Expected: '{expectedName}'. Full JSON response: {responseContent}");
            }
            
            _equipmentResult.Name.Should().Be(expectedName);
        }
        else
        {
            throw new InvalidOperationException("No equipment result available");
        }
    }

    [Then(@"the equipment value should be ""(.*)""")]
    public void ThenTheEquipmentValueShouldBe(string expectedValue)
    {
        _equipmentResult.Should().NotBeNull();
        _equipmentResult!.Name.Should().Be(expectedValue);
    }

    [Then(@"the equipment should be active")]
    public void ThenTheEquipmentShouldBeActive()
    {
        _equipmentResult.Should().NotBeNull();
        _equipmentResult!.IsActive.Should().BeTrue();
    }

    [Then(@"I should receive the created equipment")]
    public Task ThenIShouldReceiveTheCreatedEquipment()
    {
        _lastResponse.Should().NotBeNull();
        
        // Get the stored content from ScenarioContext
        var responseContent = _scenarioContext.GetLastResponseContent();
        
        // First check if we have content
        responseContent.Should().NotBeNullOrEmpty("Response content should not be null or empty");
        
        // Deserialize from the string
        _equipmentResult = JsonSerializer.Deserialize<EquipmentDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _equipmentResult.Should().NotBeNull($"Failed to deserialize response: {responseContent}");
        
        // Check if Name is null and add more context
        if (_equipmentResult != null && _equipmentResult.Name == null)
        {
            throw new Exception($"Equipment Name is null! Full response: {responseContent}. Deserialized object - Id: {_equipmentResult.Id}, IsActive: {_equipmentResult.IsActive}, CreatedAt: {_equipmentResult.CreatedAt}");
        }
        
        // Store for cleanup
        if (_equipmentResult != null)
        {
            _createdEquipmentIds.Add(_equipmentResult.Id);
            _scenarioContext.Set(_equipmentResult.Id, "CreatedEquipmentId");
        }
        
        return Task.CompletedTask;
    }

    [Then(@"I should receive the updated equipment")]
    public Task ThenIShouldReceiveTheUpdatedEquipment()
    {
        _lastResponse.Should().NotBeNull();
        
        // Get the stored content from ScenarioContext
        var responseContent = _scenarioContext.GetLastResponseContent();
        Console.WriteLine($"Update Equipment Response: {responseContent}");
        
        // Deserialize from the string
        _equipmentResult = JsonSerializer.Deserialize<EquipmentDto>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        _equipmentResult.Should().NotBeNull();
        
        return Task.CompletedTask;
    }

    [Then(@"the equipment created timestamp should be set")]
    public void ThenTheEquipmentCreatedTimestampShouldBeSet()
    {
        _equipmentResult.Should().NotBeNull();
        _equipmentResult!.CreatedAt.Should().BeAfter(DateTime.MinValue);
    }

    [Then(@"the equipment updated timestamp should be null")]
    public void ThenTheEquipmentUpdatedTimestampShouldBeNull()
    {
        _equipmentResult.Should().NotBeNull();
        _equipmentResult!.UpdatedAt.Should().BeNull();
    }

    [Then(@"the equipment updated timestamp should be set")]
    public void ThenTheEquipmentUpdatedTimestampShouldBeSet()
    {
        _equipmentResult.Should().NotBeNull();
        _equipmentResult!.UpdatedAt.Should().NotBeNull();
        _equipmentResult.UpdatedAt.Should().BeAfter(DateTime.MinValue);
    }

    [Then(@"the equipment should no longer be retrievable")]
    public async Task ThenTheEquipmentShouldNoLongerBeRetrievable()
    {
        var equipmentId = _scenarioContext.Get<string>("CreatedEquipmentId");
        var httpClient = _scenarioContext.GetHttpClient();
        
        var response = await httpClient.GetAsync($"/api/ReferenceTables/Equipment/{equipmentId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [AfterScenario]
    public async Task CleanupCreatedEquipment()
    {
        if (_createdEquipmentIds.Any())
        {
            var httpClient = _scenarioContext.GetHttpClient();
            foreach (var equipmentId in _createdEquipmentIds)
            {
                try
                {
                    await httpClient.DeleteAsync($"/api/ReferenceTables/Equipment/{equipmentId}");
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}