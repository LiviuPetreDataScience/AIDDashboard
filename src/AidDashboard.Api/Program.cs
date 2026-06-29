using System.Text;
using System.Text.Json.Serialization;
using AidDashboard.Api.Middleware;
using AidDashboard.Api.Security;
using AidDashboard.Application.Abstractions;
using AidDashboard.Infrastructure;
using AidDashboard.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// Service registration
// ---------------------------------------------------------------------------

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize enums as their string names (e.g. "Admin", "Initial") for a clearer API contract.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddHttpContextAccessor();

// Infrastructure (database, password hashing, auth providers, auth service).
builder.Services.AddInfrastructure(builder.Configuration);

// API-layer security services.
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

// Resolve effective JWT settings once, supplying a development fallback key if none is configured.
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwtSettings.SigningKey))
{
    if (builder.Environment.IsDevelopment())
    {
        // Stable, non-secret key used only for local development.
        jwtSettings.SigningKey = "dev-only-signing-key-please-override-in-production-0123456789";
    }
    else
    {
        throw new InvalidOperationException(
            "Jwt:SigningKey must be configured (via configuration, environment variable or Key Vault) in production.");
    }
}
builder.Services.AddSingleton(Options.Create(jwtSettings));

// Authentication: JWT bearer tokens. Structured so an Entra ID scheme can be added later.
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Only administrators may perform write operations and manage users/reference data.
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

// During development the React dev server runs on its own origin and needs CORS access.
const string DevelopmentCorsPolicy = "DevelopmentCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevelopmentCorsPolicy, policy =>
        policy.WithOrigins("http://localhost:5173", "https://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// Apply migrations and seed reference data / admin accounts on startup.
// ---------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var database = services.GetRequiredService<AppDbContext>();
    await database.Database.MigrateAsync();

    var passwordHasher = services.GetRequiredService<IPasswordHasher>();
    await DataSeeder.SeedAsync(database, passwordHasher);
}

// ---------------------------------------------------------------------------
// HTTP pipeline
// ---------------------------------------------------------------------------

// Global error handling must wrap the whole pipeline.
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevelopmentCorsPolicy);
}
else
{
    app.UseHttpsRedirection();
}

// Serve the built React SPA (copied into wwwroot) and fall back to index.html for client routing.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
