using GetFitterGetBigger.Admin.Components;
using GetFitterGetBigger.Admin.Services.Authentication;
using GetFitterGetBigger.Admin.Services.Configuration;
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
    .AddClasses(classes => classes.AssignableTo<GetFitterGetBigger.Admin.Services.Strategies.IReferenceTableStrategy>())
    .AsImplementedInterfaces()
    .WithScopedLifetime());

// Register the reference data service with just ONE method!
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IGenericReferenceDataService, GetFitterGetBigger.Admin.Services.ReferenceDataService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Bridge for backward compatibility with old interface
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IReferenceDataService, GetFitterGetBigger.Admin.Services.ReferenceDataServiceBridge>();

// Add HttpClient for ExerciseService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IExerciseService, GetFitterGetBigger.Admin.Services.ExerciseService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add ExerciseStateService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IExerciseStateService, GetFitterGetBigger.Admin.Services.ExerciseStateService>();

// Add HttpClient for EquipmentService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IEquipmentService, GetFitterGetBigger.Admin.Services.EquipmentService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add EquipmentStateService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IEquipmentStateService, GetFitterGetBigger.Admin.Services.EquipmentStateService>();

// Add HttpClient for MuscleGroupsService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IMuscleGroupsService, GetFitterGetBigger.Admin.Services.MuscleGroupsService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add MuscleGroupsStateService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IMuscleGroupsStateService, GetFitterGetBigger.Admin.Services.MuscleGroupsStateService>();

// Add NavigationService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.INavigationService, GetFitterGetBigger.Admin.Services.NavigationService>();

// Add CacheHelperService
builder.Services.AddSingleton<GetFitterGetBigger.Admin.Services.ICacheHelperService, GetFitterGetBigger.Admin.Services.CacheHelperService>();

// Add HttpClient for ExerciseLinkService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IExerciseLinkService, GetFitterGetBigger.Admin.Services.ExerciseLinkService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add ExerciseLinkStateService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IExerciseLinkStateService, GetFitterGetBigger.Admin.Services.ExerciseLinkStateService>();

// Add ExerciseLinkValidationService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IExerciseLinkValidationService, GetFitterGetBigger.Admin.Services.ExerciseLinkValidationService>();

// Add HttpClient for ExerciseWeightTypeService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IExerciseWeightTypeService, GetFitterGetBigger.Admin.Services.ExerciseWeightTypeService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add ExerciseWeightTypeStateService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IExerciseWeightTypeStateService, GetFitterGetBigger.Admin.Services.ExerciseWeightTypeStateService>();

// Add HttpClient for WorkoutReferenceDataService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IWorkoutReferenceDataService, GetFitterGetBigger.Admin.Services.WorkoutReferenceDataService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add WorkoutReferenceDataStateService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IWorkoutReferenceDataStateService, GetFitterGetBigger.Admin.Services.WorkoutReferenceDataStateService>();

// Add HttpClient for WorkoutTemplateService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IWorkoutTemplateService, GetFitterGetBigger.Admin.Services.WorkoutTemplateService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add WorkoutTemplateStateService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IWorkoutTemplateStateService, GetFitterGetBigger.Admin.Services.WorkoutTemplateStateService>();

// Add LocalStorageService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.ILocalStorageService, GetFitterGetBigger.Admin.Services.LocalStorageService>();

// Add ToastService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IToastService, GetFitterGetBigger.Admin.Services.ToastService>();

var app = builder.Build();

// Clear all caches on startup to avoid cache collision issues
using (var scope = app.Services.CreateScope())
{
    var cacheHelper = scope.ServiceProvider.GetRequiredService<GetFitterGetBigger.Admin.Services.ICacheHelperService>();
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
