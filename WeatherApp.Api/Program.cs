using WeatherApp.Api.Models;
using WeatherApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ────────────────────────────────────────────────────────────
builder.Services.Configure<WeatherSettings>(
    builder.Configuration.GetSection("WeatherSettings"));

// ── CORS ─────────────────────────────────────────────────────────────────────
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactDev", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// ── HttpClient ────────────────────────────────────────────────────────────────
// Named HttpClient for Open-Meteo with a sensible timeout
builder.Services.AddHttpClient<IWeatherService, WeatherService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

// ── Application Services ──────────────────────────────────────────────────────
builder.Services.AddSingleton<IDateParserService, DateParserService>();

// ── ASP.NET Core Infrastructure ───────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "WeatherApp API", Version = "v1" });
});

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("ReactDev");
app.UseAuthorization();
app.MapControllers();

app.Run();
