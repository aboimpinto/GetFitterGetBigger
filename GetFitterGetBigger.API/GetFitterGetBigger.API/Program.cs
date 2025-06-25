using Microsoft.EntityFrameworkCore;
using GetFitterGetBigger.API.Models;
using GetFitterGetBigger.API;
using Olimpo.EntityFramework.Persistency;
using GetFitterGetBigger.API.Repositories.Interfaces;
using GetFitterGetBigger.API.Repositories.Implementations;
using GetFitterGetBigger.API.Services.Implementations;
using GetFitterGetBigger.API.Services.Interfaces;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using GetFitterGetBigger.API.Swagger;
using GetFitterGetBigger.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
builder.Services.AddTransient<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddTransient<IMetricTypeRepository, MetricTypeRepository>();
builder.Services.AddTransient<IMovementPatternRepository, MovementPatternRepository>();
builder.Services.AddTransient<IMuscleGroupRepository, MuscleGroupRepository>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IClaimRepository, ClaimRepository>();

// Register services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Configure JWT Authentication
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured."));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Add Swagger services
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GetFitterGetBigger API",
        Version = "v1"
    });
    
    // Add a custom document filter to group reference table controllers
    options.DocumentFilter<GetFitterGetBigger.API.Swagger.ReferenceTablesDocumentFilter>();
    
    // Add a custom operation filter to add controller name to operation ID
    options.OperationFilter<GetFitterGetBigger.API.Swagger.ControllerNameOperationFilter>();
});

var app = builder.Build();

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

app
    .MapGet("/weatherforecast/{city}", (string city) => {
        WeatherForecast forecast = new();
        string weather = forecast.GetWeatherForecast(city);
        return weather;
    })
    .WithName("GetWeatherForecast");

// Map controllers
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<JwtMiddleware>();

app.MapControllers();

app.Run();

// Make the Program class accessible to the test project
public partial class Program { }
