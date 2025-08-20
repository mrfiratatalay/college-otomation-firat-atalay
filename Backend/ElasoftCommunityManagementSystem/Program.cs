using System.Text;
using AspNetCoreRateLimit;
using ElasoftCommunityManagementSystem.Configurations;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Middleware;
using ElasoftCommunityManagementSystem.Services;
using ElasoftCommunityManagementSystem.Services.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"]);

// CORS politikasını ekleyelim
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowVueApp",
        builder =>
        {
            builder
                .WithOrigins("http://localhost:3000") // Frontend URL
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials(); // withCredentials için gerekli
        }
    );
});

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add health checks
builder.Services.AddHealthChecks();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddScoped<JwtTokenGeneratorService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<EventAuthorizationService>();
builder.Services.AddScoped<ClubExpenseAuthorizationService>();
builder.Services.AddScoped<ClubAuthorizationService>();
builder.Services.AddScoped<UserAuthorizationService>();
builder.Services.AddScoped<AdvisorAuthorizationService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IClubService, ClubService>();
builder.Services.AddScoped<IClubMembershipService, ClubMembershipService>();
builder.Services.AddScoped<IAdvisorService, AdvisorService>();
builder.Services.AddScoped<IClubExpenseService, ClubExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();

builder
    .Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("admin"));

    options.AddPolicy("RequireAdvisorRole", policy => policy.RequireRole("advisor", "admin"));

    options.AddPolicy("RequireClubPresidentRole", policy => policy.RequireRole("leader", "admin"));

    options.AddPolicy("RequireActiveUser", policy => policy.RequireAuthenticatedUser());
});

builder.Services.ConfigureSwagger();

builder.Host.UseSerilog(
    (context, configuration) =>
    {
        configuration
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Information,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            );
    }
);

// Add rate limiting services
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.EnableEndpointRateLimiting = true;
    options.StackBlockedRequests = false;
    options.HttpStatusCode = 429;
    options.RealIpHeader = "X-Real-IP";
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
{
    Endpoint = "*/api/auth/*",
    Period = "15m",
    Limit = 100, // örnek
},
        new RateLimitRule
        {
            Endpoint = "*/api/auth/*",
            Period = "60m",
            Limit = 500,
        },
    };
});

builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

var app = builder.Build();

// Resimler için klasör oluştur
var announcementImagesPath = Path.Combine(builder.Environment.WebRootPath, "images", "announcements");
if (!Directory.Exists(announcementImagesPath))
{
    Directory.CreateDirectory(announcementImagesPath);
}

// Exception handling middleware'ini ilk sıraya ekliyoruz
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS middleware'ini UseExceptionHandling'den sonra kullanıyoruz
app.UseCors("AllowVueApp");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Statik dosyaları servis et
app.UseStaticFiles();

app.UseSerilogRequestLogging();

// Add IpRateLimiting middleware
app.UseIpRateLimiting();

// Map endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
