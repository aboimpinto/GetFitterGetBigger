using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GetFitterGetBigger.API.Swagger
{
    // Add custom document filter for reference tables
    public class ReferenceTablesDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Create a tag for ReferenceTables
            var referenceTablesTag = new OpenApiTag
            {
                Name = "ReferenceTables",
                Description = "Reference data tables"
            };
            
            // Add the tag to the document if it doesn't exist
            if (!swaggerDoc.Tags.Any(t => t.Name == "ReferenceTables"))
            {
                swaggerDoc.Tags.Add(referenceTablesTag);
            }
            
            // Create tags for each reference table controller with hierarchical naming
            var referenceTableTags = new List<OpenApiTag>
            {
                new() { Name = "ReferenceTables/BodyParts", Description = "Body parts reference data" },
                new() { Name = "ReferenceTables/DifficultyLevels", Description = "Difficulty levels reference data" },
                new() { Name = "ReferenceTables/Equipment", Description = "Equipment reference data" },
                new() { Name = "ReferenceTables/ExecutionProtocols", Description = "Execution protocols reference data" },
                new() { Name = "ReferenceTables/ExerciseTypes", Description = "Exercise types reference data" },
                new() { Name = "ReferenceTables/ExerciseWeightTypes", Description = "Exercise weight types reference data" },
                new() { Name = "ReferenceTables/KineticChainTypes", Description = "Kinetic chain types reference data" },
                new() { Name = "ReferenceTables/MetricTypes", Description = "Metric types reference data" },
                new() { Name = "ReferenceTables/MovementPatterns", Description = "Movement patterns reference data" },
                new() { Name = "ReferenceTables/MuscleGroups", Description = "Muscle groups reference data" },
                new() { Name = "ReferenceTables/MuscleRoles", Description = "Muscle roles reference data" },
                new() { Name = "ReferenceTables/WorkoutCategories", Description = "Workout categories reference data" },
                new() { Name = "ReferenceTables/WorkoutObjectives", Description = "Workout objectives reference data" }
            };
            
            // Add all tags to the document
            foreach (var tag in referenceTableTags)
            {
                swaggerDoc.Tags.Add(tag);
            }
        }
    }

    // Add custom operation filter to add controller name to operation ID
    public class ControllerNameOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"];
            
            // List of all reference table controllers
            var referenceTableControllers = new HashSet<string>
            {
                "BodyParts", "DifficultyLevels", "Equipment", "ExecutionProtocols",
                "ExerciseTypes", "ExerciseWeightTypes", "KineticChainTypes",
                "MetricTypes", "MovementPatterns", "MuscleGroups", "MuscleRoles",
                "WorkoutCategories", "WorkoutObjectives"
            };
            
            if (controllerName != null && referenceTableControllers.Contains(controllerName))
            {
                // Set the tag to the hierarchical tag name
                operation.Tags.Clear();
                operation.Tags.Add(new OpenApiTag { Name = $"ReferenceTables/{controllerName}" });
                
                // Don't add controller name to operation ID or summary - keep them clean
            }
        }
    }
}
