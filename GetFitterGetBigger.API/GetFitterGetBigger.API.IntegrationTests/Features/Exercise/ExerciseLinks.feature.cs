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
    public partial class ExerciseLinksIntegrationFeature : object, Xunit.IClassFixture<ExerciseLinksIntegrationFeature.FixtureData>, Xunit.IAsyncLifetime
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
        public ExerciseLinksIntegrationFeature(ExerciseLinksIntegrationFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }
        
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunnerForAssembly(null, TechTalk.SpecFlow.xUnit.SpecFlowPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId());
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/Exercise", "Exercise Links Integration", "    As a fitness system\n    I want to manage exercise links between exercises\n   " +
                    " So that I can create proper workout sequences with warmups and cooldowns", ProgrammingLanguage.CSharp, featureTags);
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
            await testRunner.GivenAsync("the system has been initialized with seed data", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
        }
        
        async System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
        {
            await this.TestInitializeAsync();
        }
        
        async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
        {
            await this.TestTearDownAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Create link with valid data should succeed")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Links Integration")]
        [Xunit.TraitAttribute("Description", "Create link with valid data should succeed")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "links")]
        [Xunit.TraitAttribute("Category", "integration")]
        [Xunit.TraitAttribute("Category", "create")]
        public async System.Threading.Tasks.Task CreateLinkWithValidDataShouldSucceed()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "links",
                    "integration",
                    "create"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create link with valid data should succeed", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I have a workout exercise named \"Barbell Squat\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("I have an exercise named \"Air Squat\" with exercise types \"Workout,Warmup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync("I create an exercise link from \"Barbell Squat\" to \"Air Squat\" with link type \"War" +
                        "mup\" and display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the exercise link should be created successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the link should have target exercise \"Air Squat\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("the link should have link type \"Warmup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("the link should have display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("the link should be active", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Create link with non-workout source should fail")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Links Integration")]
        [Xunit.TraitAttribute("Description", "Create link with non-workout source should fail")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "links")]
        [Xunit.TraitAttribute("Category", "integration")]
        [Xunit.TraitAttribute("Category", "validation")]
        public async System.Threading.Tasks.Task CreateLinkWithNon_WorkoutSourceShouldFail()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "links",
                    "integration",
                    "validation"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create link with non-workout source should fail", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I have an exercise named \"Warmup Only Exercise\" with exercise types \"Warmup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("I have an exercise named \"Target Warmup\" with exercise types \"Warmup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync("I create an exercise link from \"Warmup Only Exercise\" to \"Target Warmup\" with lin" +
                        "k type \"Warmup\" and display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the request should fail with bad request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the response should contain \"Source exercise must be of type \'Workout\'\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Create link with mismatched target type should fail")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Links Integration")]
        [Xunit.TraitAttribute("Description", "Create link with mismatched target type should fail")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "links")]
        [Xunit.TraitAttribute("Category", "integration")]
        [Xunit.TraitAttribute("Category", "validation")]
        public async System.Threading.Tasks.Task CreateLinkWithMismatchedTargetTypeShouldFail()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "links",
                    "integration",
                    "validation"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create link with mismatched target type should fail", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I have a workout exercise named \"Source Workout\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("I have an exercise named \"Cooldown Exercise\" with exercise types \"Cooldown\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync("I create an exercise link from \"Source Workout\" to \"Cooldown Exercise\" with link " +
                        "type \"Warmup\" and display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the request should fail with bad request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the response should contain \"Target exercise must be of type \'Warmup\'\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Create link with rest exercise should fail")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Links Integration")]
        [Xunit.TraitAttribute("Description", "Create link with rest exercise should fail")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "links")]
        [Xunit.TraitAttribute("Category", "integration")]
        [Xunit.TraitAttribute("Category", "validation")]
        public async System.Threading.Tasks.Task CreateLinkWithRestExerciseShouldFail()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "links",
                    "integration",
                    "validation"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Create link with rest exercise should fail", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I have a workout exercise named \"Rest Link Source\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("I have a rest exercise named \"Rest Period\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync("I create an exercise link from \"Rest Link Source\" to \"Rest Period\" with link type" +
                        " \"Warmup\" and display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the request should fail with bad request", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the response should contain \"Target exercise must be of type \'Warmup\'\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get links with filters should return correct results")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Links Integration")]
        [Xunit.TraitAttribute("Description", "Get links with filters should return correct results")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "links")]
        [Xunit.TraitAttribute("Category", "integration")]
        [Xunit.TraitAttribute("Category", "query")]
        public async System.Threading.Tasks.Task GetLinksWithFiltersShouldReturnCorrectResults()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "links",
                    "integration",
                    "query"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get links with filters should return correct results", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I have a workout exercise named \"Get Links Source\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("I have an exercise named \"Warmup Target\" with exercise types \"Workout,Warmup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("I have an exercise named \"Cooldown Target\" with exercise types \"Workout,Cooldown\"" +
                        "", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("I have created a link from \"Get Links Source\" to \"Warmup Target\" with link type \"" +
                        "Warmup\" and display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("I have created a link from \"Get Links Source\" to \"Cooldown Target\" with link type" +
                        " \"Cooldown\" and display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync("I get all links for exercise \"Get Links Source\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("I should receive 2 exercise links", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.WhenAsync("I get links for exercise \"Get Links Source\" filtered by link type \"Warmup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("I should receive 1 exercise link", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the first link should have link type \"Warmup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Update link with valid data should succeed")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Links Integration")]
        [Xunit.TraitAttribute("Description", "Update link with valid data should succeed")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "links")]
        [Xunit.TraitAttribute("Category", "integration")]
        [Xunit.TraitAttribute("Category", "update")]
        public async System.Threading.Tasks.Task UpdateLinkWithValidDataShouldSucceed()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "links",
                    "integration",
                    "update"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Update link with valid data should succeed", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I have a workout exercise named \"Update Link Source\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("I have an exercise named \"Update Link Target\" with exercise types \"Workout,Warmup" +
                        "\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("I have created a link from \"Update Link Source\" to \"Update Link Target\" with link" +
                        " type \"Warmup\" and display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync("I update the exercise link to have display order 5 and active status false", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the link should be updated successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the link should have display order 5", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("the link should not be active", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Delete link with valid data should succeed")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Links Integration")]
        [Xunit.TraitAttribute("Description", "Delete link with valid data should succeed")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "links")]
        [Xunit.TraitAttribute("Category", "integration")]
        [Xunit.TraitAttribute("Category", "delete")]
        public async System.Threading.Tasks.Task DeleteLinkWithValidDataShouldSucceed()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "links",
                    "integration",
                    "delete"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Delete link with valid data should succeed", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I have a workout exercise named \"Delete Link Source\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("I have an exercise named \"Delete Link Target\" with exercise types \"Workout,Warmup" +
                        "\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("I have created a link from \"Delete Link Source\" to \"Delete Link Target\" with link" +
                        " type \"Warmup\" and display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync("I delete the exercise link", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the link should be deleted successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the link should not exist in the database", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Comprehensive exercise link workflow")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Links Integration")]
        [Xunit.TraitAttribute("Description", "Comprehensive exercise link workflow")]
        [Xunit.TraitAttribute("Category", "exercise")]
        [Xunit.TraitAttribute("Category", "links")]
        [Xunit.TraitAttribute("Category", "integration")]
        [Xunit.TraitAttribute("Category", "comprehensive")]
        public async System.Threading.Tasks.Task ComprehensiveExerciseLinkWorkflow()
        {
            string[] tagsOfScenario = new string[] {
                    "exercise",
                    "links",
                    "integration",
                    "comprehensive"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Comprehensive exercise link workflow", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I have a workout exercise named \"Main Exercise\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("I have an exercise named \"Dynamic Warmup\" with exercise types \"Workout,Warmup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("I have an exercise named \"Static Stretch\" with exercise types \"Workout,Cooldown\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync("I create an exercise link from \"Main Exercise\" to \"Dynamic Warmup\" with link type" +
                        " \"Warmup\" and display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.AndAsync("I create an exercise link from \"Main Exercise\" to \"Static Stretch\" with link type" +
                        " \"Cooldown\" and display order 1", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.ThenAsync("both exercise links should be created successfully", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.WhenAsync("I get all links for exercise \"Main Exercise\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("I should receive 2 exercise links", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the links should include both \"Warmup\" and \"Cooldown\" types", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "4.0.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : object, Xunit.IAsyncLifetime
        {
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
            {
                await ExerciseLinksIntegrationFeature.FeatureSetupAsync();
            }
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
            {
                await ExerciseLinksIntegrationFeature.FeatureTearDownAsync();
            }
        }
    }
}
#pragma warning restore
#endregion
