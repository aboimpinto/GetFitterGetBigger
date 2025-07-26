using GetFitterGetBigger.Admin.Services.TableComponentStrategies;
using Microsoft.Extensions.DependencyInjection;

namespace GetFitterGetBigger.Admin.Tests.TestHelpers
{
    /// <summary>
    /// Helper class for registering table component strategies in tests.
    /// </summary>
    public static class TableComponentStrategyTestHelper
    {
        /// <summary>
        /// Registers all table component strategies for testing.
        /// Each table has its own strategy class for proper separation of concerns.
        /// </summary>
        public static void RegisterTableComponentStrategies(IServiceCollection services)
        {
            // Register all table strategies
            services.AddScoped<ITableComponentStrategy, EquipmentTableStrategy>();
            services.AddScoped<ITableComponentStrategy, ExerciseWeightTypesTableStrategy>();
            services.AddScoped<ITableComponentStrategy, MuscleGroupsTableStrategy>();
            services.AddScoped<ITableComponentStrategy, WorkoutObjectivesTableStrategy>();
            services.AddScoped<ITableComponentStrategy, WorkoutCategoriesTableStrategy>();
            services.AddScoped<ITableComponentStrategy, ExecutionProtocolsTableStrategy>();
            services.AddScoped<ITableComponentStrategy, BodyPartsTableStrategy>();
            services.AddScoped<ITableComponentStrategy, DifficultyLevelsTableStrategy>();
            services.AddScoped<ITableComponentStrategy, KineticChainTypesTableStrategy>();
            services.AddScoped<ITableComponentStrategy, MetricTypesTableStrategy>();
            services.AddScoped<ITableComponentStrategy, MovementPatternsTableStrategy>();
            services.AddScoped<ITableComponentStrategy, MuscleRolesTableStrategy>();
        }
    }
}