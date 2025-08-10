using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace GetFitterGetBigger.API.IntegrationTests.TestInfrastructure;

/// <summary>
/// Tracks database queries executed during tests by intercepting EF Core logging
/// </summary>
public class DatabaseQueryTracker : ILogger
{
    private readonly ConcurrentBag<string> _executedQueries = new();
    private readonly string _categoryName;
    
    public DatabaseQueryTracker(string categoryName)
    {
        _categoryName = categoryName;
    }
    
    public IReadOnlyList<string> ExecutedQueries => _executedQueries.ToList();
    
    public int QueryCount => _executedQueries.Count;
    
    public int GetQueryCountForTable(string tableName)
    {
        return _executedQueries.Count(q => q.Contains($"\"{tableName}\"", StringComparison.OrdinalIgnoreCase));
    }
    
    public void Reset()
    {
        _executedQueries.Clear();
    }
    
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }
    
    public bool IsEnabled(LogLevel logLevel)
    {
        // Only track actual query execution logs
        return logLevel == LogLevel.Information && _categoryName == "Microsoft.EntityFrameworkCore.Database.Command";
    }
    
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // Track executed SQL commands (EventId 20101 is CommandExecuted)
        if (eventId.Id == 20101 && state != null)
        {
            var message = formatter(state, exception);
            if (message.Contains("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                _executedQueries.Add(message);
            }
        }
    }
}

/// <summary>
/// Logger provider that creates DatabaseQueryTracker instances
/// </summary>
public class DatabaseQueryTrackerProvider : ILoggerProvider
{
    private readonly DatabaseQueryTracker _tracker;
    
    public DatabaseQueryTrackerProvider()
    {
        _tracker = new DatabaseQueryTracker("Microsoft.EntityFrameworkCore.Database.Command");
    }
    
    public DatabaseQueryTracker Tracker => _tracker;
    
    public ILogger CreateLogger(string categoryName)
    {
        // Only track EF Core database commands
        if (categoryName == "Microsoft.EntityFrameworkCore.Database.Command")
        {
            return _tracker;
        }
        
        return NullLogger.Instance;
    }
    
    public void Dispose()
    {
    }
    
    private class NullLogger : ILogger
    {
        public static readonly NullLogger Instance = new();
        
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
    }
}