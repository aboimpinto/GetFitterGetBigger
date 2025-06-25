using GetFitterGetBigger.Admin.Components;
using GetFitterGetBigger.Admin.Services.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Add authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/login";
    options.LogoutPath = "/api/auth/logout";
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "YOUR_GOOGLE_CLIENT_ID";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "YOUR_GOOGLE_CLIENT_SECRET";
    options.SaveTokens = true;
    options.Scope.Add("profile");
    options.Scope.Add("email");
    
    // Map the Google profile picture to the picture claim
    options.Events.OnCreatingTicket = context =>
    {
        // Extract the picture URL from the user info
        if (context.User.TryGetProperty("picture", out var picture))
        {
            context.Identity?.AddClaim(new System.Security.Claims.Claim("picture", picture.ToString()));
        }
        
        return Task.CompletedTask;
    };
})
.AddFacebook(options =>
{
    options.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? "YOUR_FACEBOOK_APP_ID";
    options.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? "YOUR_FACEBOOK_APP_SECRET";
    options.SaveTokens = true;
    options.Scope.Add("email");
    options.Scope.Add("public_profile");
    
    // Map the Facebook profile picture to the picture claim
    options.Events.OnCreatingTicket = context =>
    {
        // Facebook profile picture URL format
        var id = context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(id))
        {
            var pictureUrl = $"https://graph.facebook.com/{id}/picture?type=normal";
            context.Identity?.AddClaim(new System.Security.Claims.Claim("picture", pictureUrl));
        }
        
        return Task.CompletedTask;
    };
});

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
builder.Services.AddScoped<IAuthService, AuthService>();

// Add MemoryCache for caching reference data
builder.Services.AddMemoryCache();

// Add HttpClient for ReferenceDataService
builder.Services.AddHttpClient<GetFitterGetBigger.Admin.Services.IReferenceDataService, GetFitterGetBigger.Admin.Services.ReferenceDataService>();

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

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
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
