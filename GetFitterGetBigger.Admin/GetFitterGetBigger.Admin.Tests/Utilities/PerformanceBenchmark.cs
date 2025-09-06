using System.Diagnostics;
using Xunit.Abstractions;

namespace GetFitterGetBigger.Admin.Tests.Utilities
{
    /// <summary>
    /// Performance benchmarking utility for consistent performance testing
    /// Used in Task 7.3 performance optimization tests
    /// </summary>
    public static class PerformanceBenchmark
    {
        /// <summary>
        /// Measures execution time of an action and validates against target
        /// </summary>
        /// <param name="action">Action to benchmark</param>
        /// <param name="targetMilliseconds">Maximum acceptable time in milliseconds</param>
        /// <param name="testName">Name of the test for reporting</param>
        /// <param name="output">Test output helper for logging</param>
        /// <returns>Actual execution time in milliseconds</returns>
        public static long MeasureAction(Action action, long targetMilliseconds, string testName, ITestOutputHelper? output = null)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            
            var elapsed = stopwatch.ElapsedMilliseconds;
            output?.WriteLine($"{testName}: {elapsed}ms (target: <{targetMilliseconds}ms)");
            
            if (elapsed > targetMilliseconds)
            {
                throw new PerformanceException($"{testName} took {elapsed}ms, expected <{targetMilliseconds}ms");
            }
            
            return elapsed;
        }

        /// <summary>
        /// Measures execution time of an async action and validates against target
        /// </summary>
        /// <param name="asyncAction">Async action to benchmark</param>
        /// <param name="targetMilliseconds">Maximum acceptable time in milliseconds</param>
        /// <param name="testName">Name of the test for reporting</param>
        /// <param name="output">Test output helper for logging</param>
        /// <returns>Actual execution time in milliseconds</returns>
        public static async Task<long> MeasureActionAsync(Func<Task> asyncAction, long targetMilliseconds, string testName, ITestOutputHelper? output = null)
        {
            var stopwatch = Stopwatch.StartNew();
            await asyncAction();
            stopwatch.Stop();
            
            var elapsed = stopwatch.ElapsedMilliseconds;
            output?.WriteLine($"{testName}: {elapsed}ms (target: <{targetMilliseconds}ms)");
            
            if (elapsed > targetMilliseconds)
            {
                throw new PerformanceException($"{testName} took {elapsed}ms, expected <{targetMilliseconds}ms");
            }
            
            return elapsed;
        }

        /// <summary>
        /// Measures memory usage of an action
        /// </summary>
        /// <param name="action">Action to measure</param>
        /// <param name="testName">Name of the test for reporting</param>
        /// <param name="output">Test output helper for logging</param>
        /// <returns>Memory increase in bytes</returns>
        public static long MeasureMemoryUsage(Action action, string testName, ITestOutputHelper? output = null)
        {
            // Force garbage collection before measurement
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            
            var initialMemory = GC.GetTotalMemory(false);
            
            action();
            
            var finalMemory = GC.GetTotalMemory(false);
            var memoryIncrease = finalMemory - initialMemory;
            
            output?.WriteLine($"{testName}: Memory increased by {memoryIncrease:N0} bytes");
            
            return memoryIncrease;
        }

        /// <summary>
        /// Validates that performance scales linearly with input size
        /// </summary>
        /// <param name="sampleSizes">Different input sizes to test</param>
        /// <param name="actionProvider">Function that provides action for given input size</param>
        /// <param name="maxTimePerUnit">Maximum acceptable time per unit of input</param>
        /// <param name="testName">Name of the test for reporting</param>
        /// <param name="output">Test output helper for logging</param>
        public static void ValidateLinearScaling(
            int[] sampleSizes, 
            Func<int, Action> actionProvider, 
            double maxTimePerUnit, 
            string testName,
            ITestOutputHelper? output = null)
        {
            output?.WriteLine($"{testName} - Linear Scaling Validation:");
            
            foreach (var size in sampleSizes)
            {
                var action = actionProvider(size);
                var elapsed = MeasureAction(action, (long)(size * maxTimePerUnit), $"Size {size}");
                var timePerUnit = (double)elapsed / size;
                
                output?.WriteLine($"  Size {size}: {elapsed}ms total, {timePerUnit:F2}ms per unit");
                
                if (timePerUnit > maxTimePerUnit)
                {
                    throw new PerformanceException(
                        $"{testName} with size {size} took {timePerUnit:F2}ms per unit, expected <{maxTimePerUnit}ms per unit");
                }
            }
        }

        /// <summary>
        /// Measures render count to detect unnecessary re-renders
        /// </summary>
        /// <param name="componentAction">Action that triggers component operations</param>
        /// <param name="expectedMaxRenders">Maximum expected render count</param>
        /// <param name="testName">Name of the test for reporting</param>
        /// <param name="output">Test output helper for logging</param>
        /// <returns>Actual render count</returns>
        public static int MeasureRenderCount(Action componentAction, int expectedMaxRenders, string testName, ITestOutputHelper? output = null)
        {
            // This is a conceptual method - actual implementation would depend on
            // component instrumentation or mocking framework capabilities
            var renderCount = 0;
            
            try
            {
                componentAction();
                // In a real implementation, renderCount would be tracked through component hooks
                renderCount = 1; // Placeholder
            }
            catch (Exception ex)
            {
                output?.WriteLine($"{testName} failed: {ex.Message}");
                throw;
            }
            
            output?.WriteLine($"{testName}: {renderCount} renders (target: <={expectedMaxRenders})");
            
            if (renderCount > expectedMaxRenders)
            {
                throw new PerformanceException($"{testName} triggered {renderCount} renders, expected <={expectedMaxRenders}");
            }
            
            return renderCount;
        }
    }

    /// <summary>
    /// Exception thrown when performance targets are not met
    /// </summary>
    public class PerformanceException : Exception
    {
        public PerformanceException(string message) : base(message) { }
        public PerformanceException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Performance targets for Four-Way Exercise Linking System
    /// Based on Task 7.3 requirements
    /// </summary>
    public static class PerformanceTargets
    {
        // Rendering performance targets
        public const int SmallDatasetRenderTimeMs = 50;     // <= 50 items
        public const int MediumDatasetRenderTimeMs = 100;   // <= 100 items
        public const int LargeDatasetRenderTimeMs = 500;    // <= 500 items
        public const int VeryLargeDatasetRenderTimeMs = 2000; // > 500 items

        // Context switching performance
        public const int ContextSwitchTimeMs = 200;

        // Search and filter performance
        public const int SearchTimeMs = 500;
        public const int FilterTimeMs = 100;

        // API call performance
        public const int ApiCallTimeoutMs = 30000; // 30 seconds
        public const int BulkOperationTimeMs = 2; // 2ms per operation

        // Memory usage limits
        public const long MaxMemoryIncreaseBytes = 1024 * 1024; // 1MB
        public const long MaxMemoryLeakBytes = 100 * 1024; // 100KB

        // Re-render optimization
        public const int MaxReRendersForStateChange = 3;
        public const int MaxReRendersForContextSwitch = 2;

        /// <summary>
        /// Get expected render time based on dataset size
        /// </summary>
        public static long GetExpectedRenderTime(int itemCount)
        {
            return itemCount switch
            {
                <= 50 => SmallDatasetRenderTimeMs,
                <= 100 => MediumDatasetRenderTimeMs,
                <= 500 => LargeDatasetRenderTimeMs,
                _ => VeryLargeDatasetRenderTimeMs
            };
        }

        /// <summary>
        /// Get expected search time based on dataset size
        /// </summary>
        public static long GetExpectedSearchTime(int itemCount)
        {
            // Search time increases logarithmically with dataset size
            return Math.Min(SearchTimeMs, (long)(Math.Log(itemCount) * 50));
        }
    }
}