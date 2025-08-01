using GetFitterGetBigger.Admin.Components;
using GetFitterGetBigger.Admin.Services;
using GetFitterGetBigger.Admin.Services.Authentication;
using GetFitterGetBigger.Admin.Services.Configuration;
using GetFitterGetBigger.Admin.Services.DataProviders;
using GetFitterGetBigger.Admin.Services.Strategies;
using GetFitterGetBigger.Admin.Services.TableComponentStrategies;
using GetFitterGetBigger.Admin.Services.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Add authentication configuration service
builder.Services.AddSingleton<IAuthenticationConfigurationService, AuthenticationConfigurationService>();

// Add authentication
var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
});

var authConfigService = new AuthenticationConfigurationService();
authConfigService.ConfigureAuthenticationOptions(authBuilder, builder.Configuration);

// Add authorization
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Add authentication state provider
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Add authorization handler
builder.Services.AddScoped<IAuthorizationHandler, RedirectToLoginHandler>();
builder.Services.AddScoped<IAuthorizationRequirement, AuthorizationRequirement>();

// Add auth service
builder.Services.AddHttpClient<IAuthService, AuthService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add authorization state service
builder.Services.AddScoped<IAuthorizationStateService, AuthorizationStateService>();

// Add UI services
builder.Services.AddScoped<IUserProfileDisplayService, UserProfileDisplayService>();

// Add MemoryCache for caching reference data
builder.Services.AddMemoryCache();

// Register all reference table strategies
builder.Services.Scan(scan => scan
    .FromAssemblyOf<GetFitterGetBigger.Admin.Services.Strategies.ReferenceTableStrategies.BodyPartsStrategy>()
    .AddClasses(classes => classes.AssignableTo<IReferenceTableStrategy>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Register the reference data service with just ONE method!
builder.Services.AddHttpClient<IGenericReferenceDataService, ReferenceDataService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Bridge removed - all components now use IGenericReferenceDataService directly

// Register table component strategies
// Each table has its own strategy class for proper separation of concerns
builder.Services.AddScoped<ITableComponentStrategy, EquipmentTableStrategy>();
builder.Services.AddScoped<ITableComponentStrategy, ExerciseWeightTypesTableStrategy>();

builder.Services.AddScoped<ITableComponentStrategy, MuscleGroupsTableStrategy>();
builder.Services.AddScoped<ITableComponentStrategy, WorkoutObjectivesTableStrategy>();
builder.Services.AddScoped<ITableComponentStrategy, WorkoutCategoriesTableStrategy>();
builder.Services.AddScoped<ITableComponentStrategy, ExecutionProtocolsTableStrategy>();
builder.Services.AddScoped<ITableComponentStrategy, BodyPartsTableStrategy>();
builder.Services.AddScoped<ITableComponentStrategy, DifficultyLevelsTableStrategy>();
builder.Services.AddScoped<ITableComponentStrategy, KineticChainTypesTableStrategy>();
builder.Services.AddScoped<ITableComponentStrategy, MetricTypesTableStrategy>();
builder.Services.AddScoped<ITableComponentStrategy, MovementPatternsTableStrategy>();
builder.Services.AddScoped<ITableComponentStrategy, MuscleRolesTableStrategy>();

// Add TableComponentRegistry for reference table routing
builder.Services.AddScoped<ITableComponentRegistry, TableComponentRegistry>();

// Add HttpClient for ExerciseService
builder.Services.AddHttpClient<IExerciseService, ExerciseService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add ExerciseStateService
builder.Services.AddScoped<IExerciseStateService, ExerciseStateService>();

// Add HttpClient for EquipmentService
builder.Services.AddHttpClient<IEquipmentService, EquipmentService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add EquipmentStateService
builder.Services.AddScoped<IEquipmentStateService, EquipmentStateService>();

// Add HttpClient for MuscleGroupsService
builder.Services.AddHttpClient<IMuscleGroupsService, MuscleGroupsService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add MuscleGroupsStateService
builder.Services.AddScoped<IMuscleGroupsStateService, MuscleGroupsStateService>();

// Add NavigationService
builder.Services.AddScoped<INavigationService, NavigationService>();

// Add CacheHelperService
builder.Services.AddSingleton<ICacheHelperService, CacheHelperService>();

// Add HttpClient for ExerciseLinkService
builder.Services.AddHttpClient<IExerciseLinkService, ExerciseLinkService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add ExerciseLinkStateService
builder.Services.AddScoped<IExerciseLinkStateService, ExerciseLinkStateService>();

// Add ExerciseLinkValidationService
builder.Services.AddScoped<IExerciseLinkValidationService, ExerciseLinkValidationService>();

// Add HttpClient for ExerciseWeightTypeService
builder.Services.AddHttpClient<IExerciseWeightTypeService, ExerciseWeightTypeService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add ExerciseWeightTypeStateService
builder.Services.AddScoped<IExerciseWeightTypeStateService, ExerciseWeightTypeStateService>();

// Add HttpClient for WorkoutReferenceDataService
builder.Services.AddHttpClient<IWorkoutReferenceDataService, WorkoutReferenceDataService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add WorkoutReferenceDataStateService
builder.Services.AddScoped<IWorkoutReferenceDataStateService, WorkoutReferenceDataStateService>();

// Add HttpClient for WorkoutTemplateDataProvider
builder.Services.AddHttpClient<IWorkoutTemplateDataProvider, HttpWorkoutTemplateDataProvider>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add WorkoutTemplateService with data provider
builder.Services.AddScoped<IWorkoutTemplateService, WorkoutTemplateService>();

// Add Store Event Aggregator
builder.Services.AddSingleton<GetFitterGetBigger.Admin.Services.Stores.IStoreEventAggregator, GetFitterGetBigger.Admin.Services.Stores.StoreEventAggregator>();

// Add Workout Template Stores
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.Stores.IWorkoutTemplateListStore, GetFitterGetBigger.Admin.Services.Stores.WorkoutTemplateListStore>();
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.Stores.IWorkoutTemplateFormStore, GetFitterGetBigger.Admin.Services.Stores.WorkoutTemplateFormStore>();
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.Stores.IWorkoutReferenceDataStore, GetFitterGetBigger.Admin.Services.Stores.WorkoutReferenceDataStore>();


// Add LocalStorageService
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

// Add ToastService
builder.Services.AddScoped<IToastService, ToastService>();

var app = builder.Build();

// Clear all caches on startup to avoid cache collision issues
using (var scope = app.Services.CreateScope())
{
    var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelperService>();
    cacheHelper.ClearAllCaches();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
else
{
    // Enable detailed error pages in development
    app.UseDeveloperExceptionPage();
}

// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
app.UseHsts();
app.UseHttpsRedirection();


app.UseAntiforgery();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
