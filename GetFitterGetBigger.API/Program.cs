using System;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
// Add NSwag services
builder.Services.AddOpenApiDocument(settings =>
{
    settings.DocumentName = "v1";
    settings.Title = "GetFitterGetBigger API";
    settings.Version = "v1";
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Add NSwag middleware
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app
    .MapGet("/weatherforecast/{city}", (string city) => {
        WeatherForecast forecast = new();
        string weather = forecast.GetWeatherForecast(city);
        return weather;
    })
    .WithName("GetWeatherForecast");

app.Run();
