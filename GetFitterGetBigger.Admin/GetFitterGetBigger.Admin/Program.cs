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

// Add HttpClient for ReferenceDataService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IReferenceDataService, GetFitterGetBigger.Admin.Services.ReferenceDataService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5214");
});

// Add HttpClient for ExerciseService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IExerciseService, GetFitterGetBigger.Admin.Services.ExerciseService>();

// Add ExerciseStateService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IExerciseStateService, GetFitterGetBigger.Admin.Services.ExerciseStateService>();

// Add HttpClient for EquipmentService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IEquipmentService, GetFitterGetBigger.Admin.Services.EquipmentService>();

// Add EquipmentStateService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IEquipmentStateService, GetFitterGetBigger.Admin.Services.EquipmentStateService>();

// Add HttpClient for MuscleGroupsService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IMuscleGroupsService, GetFitterGetBigger.Admin.Services.MuscleGroupsService>();

// Add MuscleGroupsStateService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.IMuscleGroupsStateService, GetFitterGetBigger.Admin.Services.MuscleGroupsStateService>();

// Add NavigationService
builder.Services.AddScoped<GetFitterGetBigger.Admin.Services.INavigationService, GetFitterGetBigger.Admin.Services.NavigationService>();

// Add CacheHelperService
builder.Services.AddSingleton<GetFitterGetBigger.Admin.Services.ICacheHelperService, GetFitterGetBigger.Admin.Services.CacheHelperService>();

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
