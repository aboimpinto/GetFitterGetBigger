
using Microsoft.EntityFrameworkCore;
using GetFitterGetBigger.API.Models;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Repositories.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.SetConfiguration;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Exercise;
using GetFitterGetBigger.API.Services.Authentication.DataServices;
using GetFitterGetBigger.API.Services.Exercise;
using GetFitterGetBigger.API.Services.Exercise.DataServices;
using GetFitterGetBigger.API.Services.Exercise.Features.Links;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.DataServices;
using GetFitterGetBigger.API.Services.Exercise.Features.Links.Handlers;
using GetFitterGetBigger.API.Services.ReferenceTables.BodyPart.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.KineticChainType.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.MetricType.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleRole.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.ExerciseWeightType.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.Equipment.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.ExecutionProtocol.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutCategory.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutObjective.DataServices;
using GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState.DataServices;
using GetFitterGetBigger.API.Services.WorkoutTemplate;
using GetFitterGetBigger.API.Services.WorkoutTemplate.DataServices;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Features.Equipment;
using GetFitterGetBigger.API.Services.WorkoutTemplate.Handlers;
using Microsoft.OpenApi.Models;
using System.Reflection;
using GetFitterGetBigger.API.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add Memory Cache
builder.Services.AddMemoryCache();

// Register cache services
// Standard cache service for lookup/enhanced reference tables (Equipment, MuscleGroup)
builder.Services.AddSingleton<ICacheService, CacheService>();
// Eternal cache service for pure reference data (BodyPart, DifficultyLevel, etc.)
builder.Services.AddSingleton<IEternalCacheService, EternalCacheService>();

// Add controllers with JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to handle records with private constructors
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.IncludeFields = true;
    });

// Add DbContext
if (builder.Environment.IsEnvironment("Testing"))
{
    // For testing, the database is configured in the test fixture
}
else
{
    builder.Services.AddDbContext<FitnessDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Register UnitOfWork
builder.Services.AddTransient<IUnitOfWorkProvider<FitnessDbContext>, UnitOfWorkProvider<FitnessDbContext>>();

// Register repositories
builder.Services.AddTransient<IDifficultyLevelRepository, DifficultyLevelRepository>();
builder.Services.AddTransient<IKineticChainTypeRepository, KineticChainTypeRepository>();
builder.Services.AddTransient<IBodyPartRepository, BodyPartRepository>();
builder.Services.AddTransient<IMuscleRoleRepository, MuscleRoleRepository>();
builder.Services.AddTransient<IExerciseTypeRepository, ExerciseTypeRepository>();
builder.Services.AddTransient<IExerciseWeightTypeRepository, ExerciseWeightTypeRepository>();
builder.Services.AddTransient<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddTransient<IMetricTypeRepository, MetricTypeRepository>();
builder.Services.AddTransient<IMovementPatternRepository, MovementPatternRepository>();
builder.Services.AddTransient<IMuscleGroupRepository, MuscleGroupRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IClaimRepository, ClaimRepository>();
builder.Services.AddTransient<IExerciseRepository, ExerciseRepository>();
builder.Services.AddTransient<IExerciseLinkRepository, ExerciseLinkRepository>();
builder.Services.AddTransient<IWorkoutObjectiveRepository, WorkoutObjectiveRepository>();
builder.Services.AddTransient<IWorkoutCategoryRepository, WorkoutCategoryRepository>();
builder.Services.AddTransient<IExecutionProtocolRepository, ExecutionProtocolRepository>();
builder.Services.AddTransient<IWorkoutStateRepository, WorkoutStateRepository>();
builder.Services.AddTransient<IWorkoutTemplateRepository, WorkoutTemplateRepository>();
builder.Services.AddTransient<IWorkoutTemplateExerciseRepository, WorkoutTemplateExerciseRepository>();
builder.Services.AddTransient<ISetConfigurationRepository, SetConfigurationRepository>();

// Register services
builder.Services.AddTransient<IExerciseService, ExerciseService>();

// Exercise DataServices - Data Access Layer
builder.Services.AddTransient<IExerciseQueryDataService, ExerciseQueryDataService>();
builder.Services.AddTransient<IExerciseCommandDataService, ExerciseCommandDataService>();

// ExerciseLink DataServices - Data Access Layer  
builder.Services.AddTransient<IExerciseLinkQueryDataService, ExerciseLinkQueryDataService>();
builder.Services.AddTransient<IExerciseLinkCommandDataService, ExerciseLinkCommandDataService>();

// ExerciseLink Handlers - Business Logic Handlers
builder.Services.AddTransient<IBidirectionalLinkHandler, BidirectionalLinkHandler>();
builder.Services.AddTransient<ILinkValidationHandler, LinkValidationHandler>();

builder.Services.AddTransient<IExerciseLinkService, ExerciseLinkService>();
builder.Services.AddTransient<IWorkoutTemplateService, WorkoutTemplateService>();
builder.Services.AddTransient<IEquipmentRequirementsService, EquipmentRequirementsService>();
builder.Services.AddTransient<IWorkoutTemplateExerciseService, WorkoutTemplateExerciseService>();
builder.Services.AddTransient<ISetConfigurationService, SetConfigurationService>();

// WorkoutTemplate DataServices - Data Access Layer
builder.Services.AddTransient<IWorkoutTemplateQueryDataService, WorkoutTemplateQueryDataService>();
builder.Services.AddTransient<IWorkoutTemplateCommandDataService, WorkoutTemplateCommandDataService>();
builder.Services.AddTransient<IWorkoutTemplateExerciseCommandDataService, WorkoutTemplateExerciseCommandDataService>();

// Workout Template Handlers (internal components - no interfaces)
builder.Services.AddTransient<StateTransitionHandler>();
builder.Services.AddTransient<DuplicationHandler>();
builder.Services.AddTransient<SuggestionHandler>();

// Register reference table services
// All reference tables now use direct cache integration with IEternalCacheService
// No longer using PureReferenceService wrapper pattern

// BodyPart Services - Refactored Pattern
builder.Services.AddTransient<IBodyPartDataService, BodyPartDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.BodyPart.IBodyPartService, GetFitterGetBigger.API.Services.ReferenceTables.BodyPart.BodyPartService>();

// DifficultyLevel Services - Refactored Pattern
builder.Services.AddTransient<IDifficultyLevelDataService, DifficultyLevelDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel.IDifficultyLevelService, GetFitterGetBigger.API.Services.ReferenceTables.DifficultyLevel.DifficultyLevelService>();

// ExerciseType Services - Refactored Pattern
builder.Services.AddTransient<IExerciseTypeDataService, ExerciseTypeDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType.IExerciseTypeService, GetFitterGetBigger.API.Services.ReferenceTables.ExerciseType.ExerciseTypeService>();

// KineticChainType Services - Refactored Pattern
builder.Services.AddTransient<IKineticChainTypeDataService, KineticChainTypeDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.KineticChainType.IKineticChainTypeService, GetFitterGetBigger.API.Services.ReferenceTables.KineticChainType.KineticChainTypeService>();

// MetricType Services - Refactored Pattern
builder.Services.AddTransient<IMetricTypeDataService, MetricTypeDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.MetricType.IMetricTypeService, GetFitterGetBigger.API.Services.ReferenceTables.MetricType.MetricTypeService>();

// MovementPattern Services - Refactored Pattern
builder.Services.AddTransient<IMovementPatternDataService, MovementPatternDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern.IMovementPatternService, GetFitterGetBigger.API.Services.ReferenceTables.MovementPattern.MovementPatternService>();

// MuscleRole Services - Refactored Pattern
builder.Services.AddTransient<IMuscleRoleDataService, MuscleRoleDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.MuscleRole.IMuscleRoleService, GetFitterGetBigger.API.Services.ReferenceTables.MuscleRole.MuscleRoleService>();

// ExerciseWeightType Services - Refactored Pattern
builder.Services.AddTransient<IExerciseWeightTypeDataService, ExerciseWeightTypeDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.ExerciseWeightType.IExerciseWeightTypeService, GetFitterGetBigger.API.Services.ReferenceTables.ExerciseWeightType.ExerciseWeightTypeService>();

// MuscleGroup Services - Refactored Pattern (CRUD enabled)
builder.Services.AddTransient<IMuscleGroupDataService, MuscleGroupDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup.IMuscleGroupService, GetFitterGetBigger.API.Services.ReferenceTables.MuscleGroup.MuscleGroupService>();

// Equipment Services - Refactored Pattern (CRUD enabled)
builder.Services.AddTransient<IEquipmentDataService, EquipmentDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.Equipment.IEquipmentService, GetFitterGetBigger.API.Services.ReferenceTables.Equipment.EquipmentService>();

// ExecutionProtocol Services - Refactored Pattern
builder.Services.AddTransient<IExecutionProtocolDataService, ExecutionProtocolDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.ExecutionProtocol.IExecutionProtocolService, GetFitterGetBigger.API.Services.ReferenceTables.ExecutionProtocol.ExecutionProtocolService>();

// WorkoutCategory Services - Refactored Pattern
builder.Services.AddTransient<IWorkoutCategoryDataService, WorkoutCategoryDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.WorkoutCategory.IWorkoutCategoryService, GetFitterGetBigger.API.Services.ReferenceTables.WorkoutCategory.WorkoutCategoryService>();

// WorkoutObjective Services - Refactored Pattern
builder.Services.AddTransient<IWorkoutObjectiveDataService, WorkoutObjectiveDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.WorkoutObjective.IWorkoutObjectiveService, GetFitterGetBigger.API.Services.ReferenceTables.WorkoutObjective.WorkoutObjectiveService>();

// WorkoutState Services - Refactored Pattern
builder.Services.AddTransient<IWorkoutStateDataService, WorkoutStateDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState.IWorkoutStateService, GetFitterGetBigger.API.Services.ReferenceTables.WorkoutState.WorkoutStateService>();


// Authentication Services - Refactored Pattern
builder.Services.AddTransient<IUserQueryDataService, UserQueryDataService>();
builder.Services.AddTransient<IUserCommandDataService, UserCommandDataService>();
builder.Services.AddTransient<IClaimQueryDataService, ClaimQueryDataService>();
builder.Services.AddTransient<IClaimCommandDataService, ClaimCommandDataService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.Authentication.IAuthService, GetFitterGetBigger.API.Services.Authentication.AuthService>();
builder.Services.AddTransient<GetFitterGetBigger.API.Services.Authentication.IClaimService, GetFitterGetBigger.API.Services.Authentication.ClaimService>();

// Register old interfaces for backward compatibility (will be removed later)
// For now, remove these registrations until we create adapters
// builder.Services.AddTransient<GetFitterGetBigger.API.Services.Interfaces.IAuthService, AuthServiceAdapter>();
// builder.Services.AddTransient<GetFitterGetBigger.API.Services.Interfaces.IClaimService, ClaimServiceAdapter>();

// Register JWT service
builder.Services.AddTransient<IJwtService, JwtService>();

// Register validation services
builder.Services.AddTransient<IExerciseWeightValidator, ExerciseWeightValidator>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();


// Add Swagger services
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GetFitterGetBigger API",
        Version = "v1",
        Description = "API for fitness tracking and exercise management"
    });

    // Add a custom document filter to group reference table controllers
    options.DocumentFilter<GetFitterGetBigger.API.Swagger.ReferenceTablesDocumentFilter>();
    
    // Add a custom operation filter to add controller name to operation ID
    options.OperationFilter<GetFitterGetBigger.API.Swagger.ControllerNameOperationFilter>();
    
    // Include XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Apply database migrations on startup
// Skip migrations in test environment to prevent test crashes
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        
        try
        {
            logger.LogInformation("Starting database migration check...");
            
            var context = scope.ServiceProvider.GetRequiredService<FitnessDbContext>();
            
            // This will create the database if it doesn't exist and apply all pending migrations
            context.Database.Migrate();
            
            logger.LogInformation("Database migration completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database migration failed. Application cannot start with an invalid database schema.");
            
            // Exit gracefully to prevent running with mismatched schema
            Environment.Exit(1);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add Swagger middleware
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GetFitterGetBigger API v1");
        options.RoutePrefix = "swagger";
        
        // Configure Swagger UI to group by tags
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        options.DefaultModelsExpandDepth(-1); // Hide models section
    });
}

// Map controllers
app.MapControllers();

app.Run();

// Make the Program class accessible to the test project
public partial class Program { }
