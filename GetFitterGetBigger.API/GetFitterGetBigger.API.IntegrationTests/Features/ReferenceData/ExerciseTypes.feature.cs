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
namespace GetFitterGetBigger.API.IntegrationTests.Features.ReferenceData
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "4.0.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public partial class ExerciseTypesReferenceDataFeature : object, Xunit.IClassFixture<ExerciseTypesReferenceDataFeature.FixtureData>, Xunit.IAsyncLifetime
    {
        
        private static TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
        private Xunit.Abstractions.ITestOutputHelper _testOutputHelper;
        
        public ExerciseTypesReferenceDataFeature(ExerciseTypesReferenceDataFeature.FixtureData fixtureData, Xunit.Abstractions.ITestOutputHelper testOutputHelper)
        {
            this._testOutputHelper = testOutputHelper;
        }
        
        public static async System.Threading.Tasks.Task FeatureSetupAsync()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunnerForAssembly(null, TechTalk.SpecFlow.xUnit.SpecFlowPlugin.XUnitParallelWorkerTracker.Instance.GetWorkerId());
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features/ReferenceData", "Exercise Types Reference Data", "  As a system user\n  I want to access exercise types reference data\n  So that I c" +
                    "an use them in exercise management", ProgrammingLanguage.CSharp, featureTags);
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
            await testRunner.GivenAsync("the database is empty", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
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
        
        [Xunit.SkippableFactAttribute(DisplayName="Get all exercise types")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Types Reference Data")]
        [Xunit.TraitAttribute("Description", "Get all exercise types")]
        [Xunit.TraitAttribute("Category", "reference-data")]
        [Xunit.TraitAttribute("Category", "smoke")]
        public async System.Threading.Tasks.Task GetAllExerciseTypes()
        {
            string[] tagsOfScenario = new string[] {
                    "reference-data",
                    "smoke"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get all exercise types", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.WhenAsync("I send a GET request to \"/api/ReferenceTables/ExerciseTypes\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 200", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the response should be a JSON array", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get exercise type by valid ID")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Types Reference Data")]
        [Xunit.TraitAttribute("Description", "Get exercise type by valid ID")]
        [Xunit.TraitAttribute("Category", "reference-data")]
        public async System.Threading.Tasks.Task GetExerciseTypeByValidID()
        {
            string[] tagsOfScenario = new string[] {
                    "reference-data"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get exercise type by valid ID", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I send a GET request to \"/api/ReferenceTables/ExerciseTypes\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("the response contains at least 1 item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("I store the first item from the response as \"firstExerciseType\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync("I send a GET request to \"/api/ReferenceTables/ExerciseTypes/<firstExerciseType.id" +
                        ">\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 200", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the response should have property \"id\" with value \"<firstExerciseType.id>\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("the response should have property \"value\" with value \"<firstExerciseType.value>\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get exercise type by invalid ID format returns bad request")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Types Reference Data")]
        [Xunit.TraitAttribute("Description", "Get exercise type by invalid ID format returns bad request")]
        [Xunit.TraitAttribute("Category", "reference-data")]
        [Xunit.TraitAttribute("Category", "validation")]
        public async System.Threading.Tasks.Task GetExerciseTypeByInvalidIDFormatReturnsBadRequest()
        {
            string[] tagsOfScenario = new string[] {
                    "reference-data",
                    "validation"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get exercise type by invalid ID format returns bad request", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.WhenAsync("I send a GET request to \"/api/ReferenceTables/ExerciseTypes/invalid-id\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 400", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get exercise type by non-existent ID returns not found")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Types Reference Data")]
        [Xunit.TraitAttribute("Description", "Get exercise type by non-existent ID returns not found")]
        [Xunit.TraitAttribute("Category", "reference-data")]
        [Xunit.TraitAttribute("Category", "validation")]
        public async System.Threading.Tasks.Task GetExerciseTypeByNon_ExistentIDReturnsNotFound()
        {
            string[] tagsOfScenario = new string[] {
                    "reference-data",
                    "validation"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get exercise type by non-existent ID returns not found", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.WhenAsync("I send a GET request to \"/api/ReferenceTables/ExerciseTypes/exercisetype-99999999" +
                        "-9999-9999-9999-999999999999\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 404", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get exercise type by valid value")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Types Reference Data")]
        [Xunit.TraitAttribute("Description", "Get exercise type by valid value")]
        [Xunit.TraitAttribute("Category", "reference-data")]
        public async System.Threading.Tasks.Task GetExerciseTypeByValidValue()
        {
            string[] tagsOfScenario = new string[] {
                    "reference-data"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get exercise type by valid value", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I send a GET request to \"/api/ReferenceTables/ExerciseTypes\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("the response contains at least 1 item", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.AndAsync("I store the first item from the response as \"firstExerciseType\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync("I send a GET request to \"/api/ReferenceTables/ExerciseTypes/ByValue/<firstExercis" +
                        "eType.value>\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 200", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the response should have property \"value\" with value \"<firstExerciseType.value>\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableFactAttribute(DisplayName="Get exercise type by non-existent value returns not found")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Types Reference Data")]
        [Xunit.TraitAttribute("Description", "Get exercise type by non-existent value returns not found")]
        [Xunit.TraitAttribute("Category", "reference-data")]
        [Xunit.TraitAttribute("Category", "validation")]
        public async System.Threading.Tasks.Task GetExerciseTypeByNon_ExistentValueReturnsNotFound()
        {
            string[] tagsOfScenario = new string[] {
                    "reference-data",
                    "validation"};
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get exercise type by non-existent value returns not found", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.WhenAsync("I send a GET request to \"/api/ReferenceTables/ExerciseTypes/ByValue/InvalidType\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 404", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [Xunit.SkippableTheoryAttribute(DisplayName="Get exercise type by value with different casing")]
        [Xunit.TraitAttribute("FeatureTitle", "Exercise Types Reference Data")]
        [Xunit.TraitAttribute("Description", "Get exercise type by value with different casing")]
        [Xunit.TraitAttribute("Category", "reference-data")]
        [Xunit.TraitAttribute("Category", "validation")]
        [Xunit.InlineDataAttribute("Warmup", new string[0])]
        [Xunit.InlineDataAttribute("warmup", new string[0])]
        [Xunit.InlineDataAttribute("WARMUP", new string[0])]
        [Xunit.InlineDataAttribute("WaRmUp", new string[0])]
        public async System.Threading.Tasks.Task GetExerciseTypeByValueWithDifferentCasing(string casing, string[] exampleTags)
        {
            string[] @__tags = new string[] {
                    "reference-data",
                    "validation"};
            if ((exampleTags != null))
            {
                @__tags = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Concat(@__tags, exampleTags));
            }
            string[] tagsOfScenario = @__tags;
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            argumentsOfScenario.Add("casing", casing);
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Get exercise type by value with different casing", null, tagsOfScenario, argumentsOfScenario, featureTags);
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                await this.ScenarioStartAsync();
                await this.FeatureBackgroundAsync();
                await testRunner.GivenAsync("I send a GET request to \"/api/ReferenceTables/ExerciseTypes\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
                await testRunner.AndAsync("the response contains an item with value \"Warmup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
                await testRunner.WhenAsync(string.Format("I send a GET request to \"/api/ReferenceTables/ExerciseTypes/ByValue/{0}\"", casing), ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
                await testRunner.ThenAsync("the response status should be 200", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
                await testRunner.AndAsync("the response should have property \"value\" with value \"Warmup\"", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
            }
            await this.ScenarioCleanupAsync();
        }
        
        [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "4.0.0.0")]
        [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
        public class FixtureData : object, Xunit.IAsyncLifetime
        {
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.InitializeAsync()
            {
                await ExerciseTypesReferenceDataFeature.FeatureSetupAsync();
            }
            
            async System.Threading.Tasks.Task Xunit.IAsyncLifetime.DisposeAsync()
            {
                await ExerciseTypesReferenceDataFeature.FeatureTearDownAsync();
            }
        }
    }
}
#pragma warning restore
#endregion
