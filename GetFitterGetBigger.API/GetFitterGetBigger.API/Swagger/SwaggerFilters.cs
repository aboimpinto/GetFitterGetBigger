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
            var bodyPartsTag = new OpenApiTag
            {
                Name = "ReferenceTables/BodyParts",
                Description = "Body parts reference data"
            };
            
            var difficultyLevelsTag = new OpenApiTag
            {
                Name = "ReferenceTables/DifficultyLevels",
                Description = "Difficulty levels reference data"
            };
            
            var kineticChainTypesTag = new OpenApiTag
            {
                Name = "ReferenceTables/KineticChainTypes",
                Description = "Kinetic chain types reference data"
            };
            
            var muscleRolesTag = new OpenApiTag
            {
                Name = "ReferenceTables/MuscleRoles",
                Description = "Muscle roles reference data"
            };
            
            // Add the tags to the document
            swaggerDoc.Tags.Add(bodyPartsTag);
            swaggerDoc.Tags.Add(difficultyLevelsTag);
            swaggerDoc.Tags.Add(kineticChainTypesTag);
            swaggerDoc.Tags.Add(muscleRolesTag);
        }
    }

    // Add custom operation filter to add controller name to operation ID
    public class ControllerNameOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"];
            
            if (controllerName == "BodyParts" || 
                controllerName == "DifficultyLevels" || 
                controllerName == "KineticChainTypes" || 
                controllerName == "MuscleRoles")
            {
                // Set the tag to the hierarchical tag name
                operation.Tags.Clear();
                operation.Tags.Add(new OpenApiTag { Name = $"ReferenceTables/{controllerName}" });
                
                // Add the controller name as an operation ID prefix
                operation.OperationId = $"{controllerName}_{operation.OperationId}";
                
                // Add a description to indicate the controller
                if (operation.Summary == null)
                {
                    operation.Summary = $"{controllerName} - ";
                }
                else
                {
                    operation.Summary = $"{controllerName} - {operation.Summary}";
                }
            }
        }
    }
}
