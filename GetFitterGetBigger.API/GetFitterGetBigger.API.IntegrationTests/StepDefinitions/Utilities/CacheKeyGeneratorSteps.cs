using FluentAssertions;
using GetFitterGetBigger.API.Utilities;
using TechTalk.SpecFlow;

namespace GetFitterGetBigger.API.IntegrationTests.StepDefinitions.Utilities;

[Binding]
public class CacheKeyGeneratorSteps
{
    private readonly ScenarioContext _scenarioContext;
    private string _generatedCacheKey = string.Empty;
    private string _generatedGetAllKey = string.Empty;
    private string _generatedGetByIdKey = string.Empty;
    private string _generatedGetByValueKey = string.Empty;
    private string _generatedTablePatternKey = string.Empty;

    public CacheKeyGeneratorSteps(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [When(@"I generate a get all cache key for table ""(.*)""")]
    public void WhenIGenerateAGetAllCacheKeyForTable(string tableName)
    {
        _generatedCacheKey = CacheKeyGenerator.GetAllKey(tableName);
    }

    [When(@"I generate a get by ID cache key for table ""(.*)"" with ID ""(.*)""")]
    public void WhenIGenerateAGetByIdCacheKeyForTableWithId(string tableName, string id)
    {
        _generatedCacheKey = CacheKeyGenerator.GetByIdKey(tableName, id);
    }

    [When(@"I generate a get by value cache key for table ""(.*)"" with value ""(.*)""")]
    public void WhenIGenerateAGetByValueCacheKeyForTableWithValue(string tableName, string value)
    {
        _generatedCacheKey = CacheKeyGenerator.GetByValueKey(tableName, value);
    }

    [When(@"I generate a get by value cache key for table ""(.*)"" with null value")]
    public void WhenIGenerateAGetByValueCacheKeyForTableWithNullValue(string tableName)
    {
        _generatedCacheKey = CacheKeyGenerator.GetByValueKey(tableName, null!);
    }

    [When(@"I generate a table pattern cache key for table ""(.*)""")]
    public void WhenIGenerateATablePatternCacheKeyForTable(string tableName)
    {
        _generatedCacheKey = CacheKeyGenerator.GetTablePattern(tableName);
    }

    [When(@"I generate cache keys for table ""(.*)""")]
    public void WhenIGenerateCacheKeysForTable(string tableName)
    {
        _generatedGetAllKey = CacheKeyGenerator.GetAllKey(tableName);
        _generatedGetByIdKey = CacheKeyGenerator.GetByIdKey(tableName, "test-id");
        _generatedGetByValueKey = CacheKeyGenerator.GetByValueKey(tableName, "test-value");
        _generatedTablePatternKey = CacheKeyGenerator.GetTablePattern(tableName);
    }

    [Then(@"the cache key should be ""(.*)""")]
    public void ThenTheCacheKeyShouldBe(string expectedKey)
    {
        _generatedCacheKey.Should().Be(expectedKey);
    }

    [Then(@"the get all cache key should contain ""(.*)"" and start with ""(.*)"" and end with ""(.*)""")]
    public void ThenTheGetAllCacheKeyShouldContainAndStartWithAndEndWith(string tableName, string startsWith, string endsWith)
    {
        _generatedGetAllKey.Should().Contain(tableName);
        _generatedGetAllKey.Should().StartWith(startsWith);
        _generatedGetAllKey.Should().EndWith(endsWith);
    }

    [Then(@"the get by ID cache key should contain ""(.*)"" and contain ""(.*)""")]
    public void ThenTheGetByIdCacheKeyShouldContainAndContain(string tableName, string contains)
    {
        _generatedGetByIdKey.Should().Contain(tableName);
        _generatedGetByIdKey.Should().Contain(contains);
    }

    [Then(@"the get by value cache key should contain ""(.*)"" and contain ""(.*)""")]
    public void ThenTheGetByValueCacheKeyShouldContainAndContain(string tableName, string contains)
    {
        _generatedGetByValueKey.Should().Contain(tableName);
        _generatedGetByValueKey.Should().Contain(contains);
    }

    [Then(@"the table pattern cache key should start with ""(.*)"" and end with ""(.*)""")]
    public void ThenTheTablePatternCacheKeyShouldStartWithAndEndWith(string startsWith, string endsWith)
    {
        _generatedTablePatternKey.Should().StartWith(startsWith);
        _generatedTablePatternKey.Should().EndWith(endsWith);
    }
}