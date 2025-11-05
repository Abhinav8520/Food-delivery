using RecommendationService.Middleware;
using RecommendationService.Services;
using DotNetEnv;

// Load .env file from backend directory
var backendEnvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "backend", ".env");
if (File.Exists(backendEnvPath))
{
    Env.Load(backendEnvPath);
    Console.WriteLine($"Loaded .env from: {backendEnvPath}");
}
else
{
    // Try alternative path (if running from project root)
    var altPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "backend", ".env");
    if (File.Exists(altPath))
    {
        Env.Load(altPath);
        Console.WriteLine($"Loaded .env from: {altPath}");
    }
    else
    {
        Console.WriteLine("Warning: Could not find backend/.env file. Using environment variables.");
    }
}

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNodeJsBackend", policy =>
    {
        policy.WithOrigins("http://localhost:4000", "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Register HttpClient for GPT service
builder.Services.AddHttpClient<IGptService, GptService>();

// Register custom services
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IRecommendationService, RecommendationService.Services.RecommendationService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowNodeJsBackend");

// Custom JWT middleware
app.UseJwtMiddleware();

app.UseAuthorization();

app.MapControllers();

// Get port from environment or use default
var port = Environment.GetEnvironmentVariable("PORT") ?? builder.Configuration["Port"] ?? "5001";
app.Logger.LogInformation($"Recommendation Service starting on port {port}");

app.Run($"http://localhost:{port}");
