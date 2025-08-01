﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:4.0.0.0
//      SpecFlow Generator Version:4.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace GetFitterGetBigger.API.IntegrationTests.Features.Exercise
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "4.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class ExerciseBasicOperationsFeature : object, Xunit.IClassFixture<ExerciseBasicOperationsFeature.FixtureData>, Xunit.IAsyncLifetime
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
        public ExerciseBasicOperationsFeature(ExerciseBasicOperationsFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }
        
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunnerForAssembly(null, TechTalk.SpecFlow.xUnit.SpecFlowPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId());
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/Exercise", "Exercise Basic Operations", "  As a fitness application\n  I want to perform basic exercise operations\n  So tha" +
                    "t I can manage exercises efficiently", ProgrammingLanguage.CSharp, featureTags);
            await testRunner.OnFeatureStartAsync(featureInfo);
        }
        
        public static async System.Threading.Tasks.Task FeatureTearDownAsync()
        {
            string testWorkerId = testRunner.TestWorkerId;
            await testRunner.OnFeatureEndAsync();
            testRunner = null;
            TechTalk.SpecFlow.xUnit.SpecFlowPlugin.XUnitParallelWorkerTracker.Instance.ReleaseWorker(testWorkerId);
        }
        
        public async System.Threading.Tasks.Task TestInitializeAsync()
        {
        }
        
        public async System.Threading.Tasks.Task TestTearDownAsync()
        {
            await testRunner.OnScenarioEndAsync();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<Xunit.Abstractions.ITestOutputHelper>(_testOutputHelper);
        }
        
        public async System.Threading.Tasks.Task ScenarioStartAsync()
        {
            await testRunner.OnScenarioStartAsync();
        }
        
        public async System.Threading.Tasks.Task ScenarioCleanupAsync()
        {
            await testRunner.CollectScenarioErrorsAsync();
        }
        
        public virtual async System.Threading.Tasks.Task FeatureBackgroundAsync()
        {
            await testRunner.GivenAsync("I am authenticated as a user", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
            await testRunner.AndAsync("the database has reference data", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
        }
        
        async System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
        {
            await this.TestInitializeAsync();
        }
        
        async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
        {
            await this.TestTearDownAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get exercises without any exercises returns paginated list")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Basic Operations")]
        [Xunit.TraitAttribute("Description", "Get exercises without any exercises returns paginated list")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "basic")]
        public async System.Threading.Tasks.Task GetExercisesWithoutAnyExercisesReturnsPaginatedList()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "basic"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get exercises without any exercises returns paginated list", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.WhenAsync("I send a GET request to \"/api/exercises?page=1&pageSize=10\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 200", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the response should have property \"currentPage\" with value \"1\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("the response should have property \"pageSize\" with value \"10\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("the response should have property \"items\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("the response should have property \"totalCount\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("the response should have property \"totalPages\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get exercise with invalid format returns not found")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Basic Operations")]
        [Xunit.TraitAttribute("Description", "Get exercise with invalid format returns not found")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "basic")]
        [Xunit.TraitAttribute("Category", "validation")]
        public async System.Threading.Tasks.Task GetExerciseWithInvalidFormatReturnsNotFound()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "basic",
                    "validation"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get exercise with invalid format returns not found", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.WhenAsync("I send a GET request to \"/api/exercises/invalid-format\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 404", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Create exercise with empty name returns bad request")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Basic Operations")]
        [Xunit.TraitAttribute("Description", "Create exercise with empty name returns bad request")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "basic")]
        [Xunit.TraitAttribute("Category", "validation")]
        public async System.Threading.Tasks.Task CreateExerciseWithEmptyNameReturnsBadRequest()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "basic",
                    "validation"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create exercise with empty name returns bad request", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.WhenAsync("I send a POST request to \"/api/exercises\" with body:", @"{
  ""name"": """",
  ""description"": ""Test Description"",
  ""coachNotes"": [
    {
      ""text"": ""Test"",
      ""order"": 0
    }
  ],
  ""exerciseTypeIds"": [""exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e""],
  ""isUnilateral"": false,
  ""difficultyId"": ""difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b"",
  ""kineticChainId"": ""kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4"",
  ""exerciseWeightTypeId"": ""exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f"",
  ""muscleGroups"": [],
  ""equipmentIds"": [],
  ""bodyPartIds"": [],
  ""movementPatternIds"": []
}", ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 400", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Create exercise with missing muscle groups returns bad request")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Basic Operations")]
        [Xunit.TraitAttribute("Description", "Create exercise with missing muscle groups returns bad request")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "basic")]
        [Xunit.TraitAttribute("Category", "validation")]
        public async System.Threading.Tasks.Task CreateExerciseWithMissingMuscleGroupsReturnsBadRequest()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "basic",
                    "validation"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create exercise with missing muscle groups returns bad request", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.WhenAsync("I send a POST request to \"/api/exercises\" with body:", @"{
  ""name"": ""Test Exercise Missing Muscle Groups"",
  ""description"": ""Test Description"",
  ""coachNotes"": [
    {
      ""text"": ""Test"",
      ""order"": 0
    }
  ],
  ""exerciseTypeIds"": [""exercisetype-b2c3d4e5-6f7a-8b9c-0d1e-2f3a4b5c6d7e""],
  ""isUnilateral"": false,
  ""difficultyId"": ""difficultylevel-8a8adb1d-24d2-4979-a5a6-0d760e6da24b"",
  ""kineticChainId"": ""kineticchaintype-f5d5a2de-9c4e-4b87-b8c3-5d1e17d0b1f4"",
  ""exerciseWeightTypeId"": ""exerciseweighttype-b2e4d3c5-6a7b-5c8d-9e0f-1a2b3c4d5e6f"",
  ""muscleGroups"": [],
  ""equipmentIds"": [],
  ""bodyPartIds"": [],
  ""movementPatternIds"": []
}", ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 400", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "4.0.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : object, Xunit.IAsyncLifetime
        {
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
            {
                await ExerciseBasicOperationsFeature.FeatureSetupAsync();
            }
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
            {
                await ExerciseBasicOperationsFeature.FeatureTearDownAsync();
            }
        }
    }
}
#pragma warning restore
#endregion
