using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using TMDB_API;
using TMDB_API.Hubs;
using TMDB_API.Repository;
using TMDB_API.Services;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<TmdbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMovieService, MovieService>(); 
builder.Services.AddScoped<UserService>();

builder.Services.AddMemoryCache();
builder.Services.AddOpenApi();
// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(
        builder.Configuration["Redis:ConnectionString"] + ",ssl=True,abortConnect=False"
    )
);
builder.Services.Configure<HostOptions>(options =>
{
    options.BackgroundServiceExceptionBehavior =
        BackgroundServiceExceptionBehavior.Ignore;
});

//MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(
        "MongoDbSettings"
    )
);
builder.Services.AddSingleton<MessageService>();

// SignalR
builder.Services.AddSignalR();

// Background notification listener
builder.Services.AddHostedService<NotificationService>();

builder.Services.AddAuthentication("Bearer")
.AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = "TMDB-API",
                ValidAudience = "TMDB-API",

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            "SECRET_KEY_EXTREME_SIZE_HIGH_SECURITY_KEYS"
                        )
                    )
            };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken =  context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

//Rate-Limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy("UserPolicy", httpContext =>
    {
        var userId = httpContext.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: userId ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,                 // 5 requests
                Window = TimeSpan.FromSeconds(1),
                QueueLimit = 0,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
    });

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;

        context.HttpContext.Response.Headers["Retry-After"] = "2";

        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please wait before trying again."
        );
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://tmdb-lku4by7tz-kaushikramabhotlas-projects.vercel.app",
                    "https://tmdb-ui-eta.vercel.app"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowFrontend");   // ← MUST be before UseAuthentication and MapHub
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHub<ChatHub>("/chatHub");

app.Run();
