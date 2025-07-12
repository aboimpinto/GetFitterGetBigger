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

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.ReferenceData;

[Binding]
public class MuscleGroupsControllerSteps
{
    private readonly ScenarioContext _scenarioContext;
    private HttpResponseMessage? _lastResponse;
    private List<MuscleGroupDto>? _muscleGroupsCollection;
    private MuscleGroupDto? _muscleGroupResult;
    private List<ReferenceDataDto>? _bodyPartsCollection;

    public MuscleGroupsControllerSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"I have muscle groups available in the system")]
    public async Task GivenIHaveMuscleGroupsAvailableInTheSystem()
    {
        var httpClient = _scenarioContext.GetHttpClient();
        var response = await httpClient.GetAsync("/api/ReferenceTables/MuscleGroups");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _muscleGroupsCollection = await response.Content.ReadFromJsonAsync<List<MuscleGroupDto>>();
        _muscleGroupsCollection.Should().NotBeNull();
        _muscleGroupsCollection.Should().NotBeEmpty("Need muscle groups for testing");
    }

    [Given(@"I have a muscle group named ""(.*)"" in the system")]
    public async Task GivenIHaveAMuscleGroupNamedInTheSystem(string muscleGroupName)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        var response = await httpClient.GetAsync("/api/ReferenceTables/MuscleGroups");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _muscleGroupsCollection = await response.Content.ReadFromJsonAsync<List<MuscleGroupDto>>();
        _muscleGroupsCollection.Should().NotBeNull();
        
        var targetMuscleGroup = _muscleGroupsCollection!.FirstOrDefault(mg => 
            mg.Name.Equals(muscleGroupName, StringComparison.OrdinalIgnoreCase));
        
        if (targetMuscleGroup == null)
        {
            // Create the muscle group if it doesn't exist
            await GivenIHaveBodyPartsAvailableInTheSystem();
            var bodyPartId = _bodyPartsCollection!.First().Id;
            
            var createDto = new CreateMuscleGroupDto
            {
                Name = muscleGroupName,
                BodyPartId = bodyPartId
            };
            
            var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
            var createResponse = await httpClient.PostAsync("/api/ReferenceTables/MuscleGroups", content);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            
            var createdMuscleGroup = await createResponse.Content.ReadFromJsonAsync<MuscleGroupDto>();
            _scenarioContext.Set(createdMuscleGroup!.Id, "CreatedMuscleGroupId");
        }
    }

    [Given(@"I have body parts available in the system")]
    public async Task GivenIHaveBodyPartsAvailableInTheSystem()
    {
        var httpClient = _scenarioContext.GetHttpClient();
        var response = await httpClient.GetAsync("/api/ReferenceTables/BodyParts");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        _bodyPartsCollection = await response.Content.ReadFromJsonAsync<List<ReferenceDataDto>>();
        _bodyPartsCollection.Should().NotBeNull();
        _bodyPartsCollection.Should().NotBeEmpty("Need body parts for muscle group creation");
    }

    [Given(@"I have created a muscle group ""(.*)"" via API")]
    public async Task GivenIHaveCreatedAMuscleGroupViaAPI(string muscleGroupName)
    {
        await GivenIHaveBodyPartsAvailableInTheSystem();
        var bodyPartId = _bodyPartsCollection!.First().Id;
        
        var httpClient = _scenarioContext.GetHttpClient();
        var uniqueName = $"{muscleGroupName}_{Guid.NewGuid()}";
        var createDto = new CreateMuscleGroupDto
        {
            Name = uniqueName,
            BodyPartId = bodyPartId
        };
        
        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync("/api/ReferenceTables/MuscleGroups", content);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdMuscleGroup = await response.Content.ReadFromJsonAsync<MuscleGroupDto>();
        _scenarioContext.Set(createdMuscleGroup!.Id, "CreatedMuscleGroupId");
        _scenarioContext.Set(uniqueName, "CreatedMuscleGroupName");
    }

    [When(@"I get all muscle groups via API")]
    public async Task WhenIGetAllMuscleGroupsViaAPI()
    {
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.GetAsync("/api/ReferenceTables/MuscleGroups");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I get the first muscle group by ID via API")]
    public async Task WhenIGetTheFirstMuscleGroupByIdViaAPI()
    {
        _muscleGroupsCollection.Should().NotBeNull();
        _muscleGroupsCollection.Should().NotBeEmpty();
        
        var firstMuscleGroup = _muscleGroupsCollection!.First();
        _scenarioContext.Set(firstMuscleGroup, "RequestedMuscleGroup");
        
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.GetAsync($"/api/ReferenceTables/MuscleGroups/{firstMuscleGroup.Id}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I get muscle group by ID ""(.*)"" via API")]
    public async Task WhenIGetMuscleGroupByIdViaAPI(string muscleGroupId)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.GetAsync($"/api/ReferenceTables/MuscleGroups/{muscleGroupId}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I get the first muscle group by value via API")]
    public async Task WhenIGetTheFirstMuscleGroupByValueViaAPI()
    {
        _muscleGroupsCollection.Should().NotBeNull();
        _muscleGroupsCollection.Should().NotBeEmpty();
        
        var firstMuscleGroup = _muscleGroupsCollection!.First();
        _scenarioContext.Set(firstMuscleGroup.Name, "RequestedMuscleGroupValue");
        
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.GetAsync($"/api/ReferenceTables/MuscleGroups/ByValue/{firstMuscleGroup.Name}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I get muscle group by value ""(.*)"" via API")]
    public async Task WhenIGetMuscleGroupByValueViaAPI(string muscleGroupValue)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.GetAsync($"/api/ReferenceTables/MuscleGroups/ByValue/{muscleGroupValue}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I create muscle group ""(.*)"" with first available body part via API")]
    public async Task WhenICreateMuscleGroupWithFirstAvailableBodyPartViaAPI(string muscleGroupName)
    {
        _bodyPartsCollection.Should().NotBeNull();
        _bodyPartsCollection.Should().NotBeEmpty();
        
        var bodyPartId = _bodyPartsCollection!.First().Id;
        
        // For duplicate testing, use the exact name from the first creation
        string nameToUse;
        if (muscleGroupName == "DuplicateTestMuscleGroup" && _scenarioContext.TryGetValue("CreatedMuscleGroupName", out var existingName))
        {
            nameToUse = (string)existingName;
        }
        else
        {
            nameToUse = $"{muscleGroupName}_{Guid.NewGuid()}";
        }
        
        var createDto = new CreateMuscleGroupDto
        {
            Name = nameToUse,
            BodyPartId = bodyPartId
        };
        
        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.PostAsync("/api/ReferenceTables/MuscleGroups", content);
        _scenarioContext.SetLastResponse(_lastResponse);
        
        // Store for cleanup and verification only if it's a new unique name
        if (!muscleGroupName.Equals("DuplicateTestMuscleGroup") || !_scenarioContext.ContainsKey("CreatedMuscleGroupName"))
        {
            _scenarioContext.Set(nameToUse, "CreatedMuscleGroupName");
        }
    }

    [When(@"I create muscle group ""(.*)"" with body part ID ""(.*)"" via API")]
    public async Task WhenICreateMuscleGroupWithBodyPartIdViaAPI(string muscleGroupName, string bodyPartId)
    {
        var createDto = new CreateMuscleGroupDto
        {
            Name = muscleGroupName,
            BodyPartId = bodyPartId
        };
        
        var content = new StringContent(JsonSerializer.Serialize(createDto), Encoding.UTF8, "application/json");
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.PostAsync("/api/ReferenceTables/MuscleGroups", content);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I update the muscle group to name ""(.*)"" via API")]
    public async Task WhenIUpdateTheMuscleGroupToNameViaAPI(string newName)
    {
        var muscleGroupId = _scenarioContext.Get<string>("CreatedMuscleGroupId");
        _bodyPartsCollection.Should().NotBeNull();
        var bodyPartId = _bodyPartsCollection!.First().Id;
        
        var uniqueNewName = $"{newName}_{Guid.NewGuid()}";
        var updateDto = new UpdateMuscleGroupDto
        {
            Name = uniqueNewName,
            BodyPartId = bodyPartId
        };
        
        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.PutAsync($"/api/ReferenceTables/MuscleGroups/{muscleGroupId}", content);
        _scenarioContext.SetLastResponse(_lastResponse);
        
        _scenarioContext.Set(uniqueNewName, "UpdatedMuscleGroupName");
    }

    [When(@"I update muscle group ""(.*)"" to name ""(.*)"" via API")]
    public async Task WhenIUpdateMuscleGroupToNameViaAPI(string muscleGroupId, string newName)
    {
        _bodyPartsCollection.Should().NotBeNull();
        var bodyPartId = _bodyPartsCollection!.First().Id;
        
        var updateDto = new UpdateMuscleGroupDto
        {
            Name = newName,
            BodyPartId = bodyPartId
        };
        
        var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.PutAsync($"/api/ReferenceTables/MuscleGroups/{muscleGroupId}", content);
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I delete the muscle group via API")]
    public async Task WhenIDeleteTheMuscleGroupViaAPI()
    {
        var muscleGroupId = _scenarioContext.Get<string>("CreatedMuscleGroupId");
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.DeleteAsync($"/api/ReferenceTables/MuscleGroups/{muscleGroupId}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [When(@"I delete muscle group ""(.*)"" via API")]
    public async Task WhenIDeleteMuscleGroupViaAPI(string muscleGroupId)
    {
        var httpClient = _scenarioContext.GetHttpClient();
        _lastResponse = await httpClient.DeleteAsync($"/api/ReferenceTables/MuscleGroups/{muscleGroupId}");
        _scenarioContext.SetLastResponse(_lastResponse);
    }

    [Then(@"I should receive a collection of muscle groups")]
    public async Task ThenIShouldReceiveACollectionOfMuscleGroups()
    {
        _lastResponse.Should().NotBeNull();
        _muscleGroupsCollection = await _lastResponse!.Content.ReadFromJsonAsync<List<MuscleGroupDto>>();
        _muscleGroupsCollection.Should().NotBeNull();
    }

    [Then(@"I should receive the muscle group")]
    public async Task ThenIShouldReceiveTheMuscleGroup()
    {
        _lastResponse.Should().NotBeNull();
        _muscleGroupResult = await _lastResponse!.Content.ReadFromJsonAsync<MuscleGroupDto>();
        _muscleGroupResult.Should().NotBeNull();
    }

    [Then(@"I should receive the created muscle group")]
    public async Task ThenIShouldReceiveTheCreatedMuscleGroup()
    {
        _lastResponse.Should().NotBeNull();
        _muscleGroupResult = await _lastResponse!.Content.ReadFromJsonAsync<MuscleGroupDto>();
        _muscleGroupResult.Should().NotBeNull();
        
        // Store for cleanup
        _scenarioContext.Set(_muscleGroupResult!.Id, "CreatedMuscleGroupId");
    }

    [Then(@"I should receive the updated muscle group")]
    public async Task ThenIShouldReceiveTheUpdatedMuscleGroup()
    {
        _lastResponse.Should().NotBeNull();
        _muscleGroupResult = await _lastResponse!.Content.ReadFromJsonAsync<MuscleGroupDto>();
        _muscleGroupResult.Should().NotBeNull();
    }

    [Then(@"the muscle group ID should match the requested ID")]
    public void ThenTheMuscleGroupIdShouldMatchTheRequestedId()
    {
        var requestedMuscleGroup = _scenarioContext.Get<MuscleGroupDto>("RequestedMuscleGroup");
        _muscleGroupResult.Should().NotBeNull();
        _muscleGroupResult!.Id.Should().Be(requestedMuscleGroup.Id);
    }

    [Then(@"the muscle group name should match the expected name")]
    public void ThenTheMuscleGroupNameShouldMatchTheExpectedName()
    {
        var requestedMuscleGroup = _scenarioContext.Get<MuscleGroupDto>("RequestedMuscleGroup");
        _muscleGroupResult.Should().NotBeNull();
        _muscleGroupResult!.Name.Should().Be(requestedMuscleGroup.Name);
    }

    [Then(@"the muscle group name should match the requested value")]
    public void ThenTheMuscleGroupNameShouldMatchTheRequestedValue()
    {
        var requestedValue = _scenarioContext.Get<string>("RequestedMuscleGroupValue");
        _muscleGroupResult.Should().NotBeNull();
        _muscleGroupResult!.Name.Should().Be(requestedValue);
    }

    [Then(@"the muscle group name should be ""(.*)""")]
    public void ThenTheMuscleGroupNameShouldBe(string expectedName)
    {
        _muscleGroupResult.Should().NotBeNull();
        
        if (expectedName == "TestMuscleGroup")
        {
            var createdName = _scenarioContext.Get<string>("CreatedMuscleGroupName");
            _muscleGroupResult!.Name.Should().Be(createdName);
        }
        else if (expectedName == "UpdatedMuscleGroup")
        {
            var updatedName = _scenarioContext.Get<string>("UpdatedMuscleGroupName");
            _muscleGroupResult!.Name.Should().Be(updatedName);
        }
        else
        {
            _muscleGroupResult!.Name.Should().BeEquivalentTo(expectedName);
        }
    }

    [Then(@"the muscle group should be active")]
    public void ThenTheMuscleGroupShouldBeActive()
    {
        _muscleGroupResult.Should().NotBeNull();
        _muscleGroupResult!.IsActive.Should().BeTrue();
    }

    [Then(@"the muscle group created timestamp should be set")]
    public void ThenTheMuscleGroupCreatedTimestampShouldBeSet()
    {
        _muscleGroupResult.Should().NotBeNull();
        _muscleGroupResult!.CreatedAt.Should().BeAfter(DateTime.MinValue);
    }

    [Then(@"the muscle group updated timestamp should be null")]
    public void ThenTheMuscleGroupUpdatedTimestampShouldBeNull()
    {
        _muscleGroupResult.Should().NotBeNull();
        _muscleGroupResult!.UpdatedAt.Should().BeNull();
    }

    [Then(@"the muscle group updated timestamp should be set")]
    public void ThenTheMuscleGroupUpdatedTimestampShouldBeSet()
    {
        _muscleGroupResult.Should().NotBeNull();
        _muscleGroupResult!.UpdatedAt.Should().NotBeNull();
    }

    [Then(@"the muscle group should no longer be retrievable")]
    public async Task ThenTheMuscleGroupShouldNoLongerBeRetrievable()
    {
        var muscleGroupId = _scenarioContext.Get<string>("CreatedMuscleGroupId");
        var httpClient = _scenarioContext.GetHttpClient();
        
        var response = await httpClient.GetAsync($"/api/ReferenceTables/MuscleGroups/{muscleGroupId}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Then(@"I clean up the created muscle group")]
    public async Task ThenICleanUpTheCreatedMuscleGroup()
    {
        if (_scenarioContext.TryGetValue("CreatedMuscleGroupId", out string muscleGroupId))
        {
            var httpClient = _scenarioContext.GetHttpClient();
            await httpClient.DeleteAsync($"/api/ReferenceTables/MuscleGroups/{muscleGroupId}");
        }
    }
}