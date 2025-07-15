
using Microsoft.EntityFrameworkCore;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Repositories.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using GetFitterGetBigger.API.Services.Implementations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using GetFitterGetBigger.API.Swagger;
using GetFitterGetBigger.API.Configuration;
using GetFitterGetBigger.API.Validators;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add Memory Cache
builder.Services.AddMemoryCache();

// Configure cache settings
builder.Services.Configure<CacheConfiguration>(
    builder.Configuration.GetSection("CacheConfiguration"));

// Register cache service
builder.Services.AddSingleton<ICacheService, CacheService>();
// TEMPORARY: Register Empty-enabled cache service interface
// This will be removed once all services are migrated to Empty pattern
builder.Services.AddSingleton<IEmptyEnabledCacheService>(provider => 
    (IEmptyEnabledCacheService)provider.GetRequiredService<ICacheService>());

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

// Register services
builder.Services.AddTransient<IExerciseService, ExerciseService>();
builder.Services.AddTransient<IEquipmentService, EquipmentService>();
builder.Services.AddTransient<IMuscleGroupService, MuscleGroupService>();
builder.Services.AddTransient<IExerciseLinkService, ExerciseLinkService>();
builder.Services.AddTransient<IWorkoutObjectiveService, WorkoutObjectiveService>();
builder.Services.AddTransient<IWorkoutCategoryService, WorkoutCategoryService>();
builder.Services.AddTransient<IExecutionProtocolService, ExecutionProtocolService>();

// Register reference table validation services
builder.Services.AddTransient<IBodyPartService, BodyPartService>();
builder.Services.AddTransient<IMovementPatternService, MovementPatternService>();
builder.Services.AddTransient<IExerciseTypeService, ExerciseTypeService>();
builder.Services.AddTransient<IExerciseWeightTypeService, ExerciseWeightTypeService>();
builder.Services.AddTransient<IClaimService, ClaimService>();
builder.Services.AddTransient<IDifficultyLevelService, DifficultyLevelService>();
builder.Services.AddTransient<IKineticChainTypeService, KineticChainTypeService>();
builder.Services.AddTransient<IMuscleRoleService, MuscleRoleService>();

// Register authentication services
builder.Services.AddTransient<IJwtService, JwtService>();
builder.Services.AddTransient<IAuthService, AuthService>();

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
            await context.Database.MigrateAsync();
            
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
