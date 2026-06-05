using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using TaskFlowAPI.Data;
using TaskFlowAPI.Middleware;
using TaskFlowAPI.Repositories;
using TaskFlowAPI.Services;

// Configure Serilog Before Everything Else 

// Bootstrap logger catches any errors during startup before the app is fully built
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("TaskFlow API starting up...");

    var builder = WebApplication.CreateBuilder(args);


    builder.Host.UseSerilog((context, services, configuration) => configuration
        // Read base configuration from appsettings.json
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)

        // Always write to console
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")

        // Write to a rolling file — new file created each day
        // Logs folder is created automatically
        .WriteTo.File(
            path: "Logs/taskflow-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            retainedFileCountLimit: 7) // Keep 7 days of logs

        // Minimum level — ignore anything below Information
        .MinimumLevel.Information()

        // Suppress noisy ASP.NET framework logs below Warning
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information));
    
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddRateLimiter(options =>
    {
        // Global 429 response — returned when any limit is exceeded
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        // Auth policy — strictest — stops brute force on login/register
        options.AddFixedWindowLimiter("auth", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;               // 5 requests
            limiterOptions.Window = TimeSpan.FromMinutes(1); // per minute
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;                // no queuing — reject immediately
        });

        // Write policy — for POST, PUT, PATCH, DELETE
        options.AddFixedWindowLimiter("write", limiterOptions =>
        {
            limiterOptions.PermitLimit = 30;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        // Read policy — for GET endpoints
        options.AddFixedWindowLimiter("read", limiterOptions =>
        {
            limiterOptions.PermitLimit = 100;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });
    });

    builder.Services.AddApiVersioning(options =>
    {
        // Default version when none is specified
        options.DefaultApiVersion = new ApiVersion(1, 0);

        // Assume default version when client doesn't specify one
        options.AssumeDefaultVersionWhenUnspecified = true;

        // Include supported versions in response headers
        // Client sees: api-supported-versions: 1.0
        options.ReportApiVersions = true;

        // Read version from URL segment e.g. /api/v1/tasks
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        // Format version as 'v{major}' in the URL
        options.GroupNameFormat = "'v'VVV";

        // Substitute the version in the route template automatically
        options.SubstituteApiVersionInUrl = true;
    });
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<ITaskRepository, TaskRepository>();
    builder.Services.AddScoped<ITokenService, TokenService>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ReactAppPolicy", policy =>
        {
            policy.WithOrigins("http://localhost:5173") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    builder.Services.AddSwaggerGen(options =>
    {
        // Create a separate Swagger doc for each API version
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "TaskFlow API",
            Version = "v1",
            Description = "A task management REST API with JWT authentication, " +
                          "pagination, filtering, and user-scoped data.",
            Contact = new OpenApiContact
            {
                Name = "Rethabile Eric Siase",
                Url = new Uri("https://github.com/Rethabile2004")
            }
        });

        // Define the JWT Bearer security scheme
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            // What it is
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,

            // Instructions shown in Swagger UI
            Description = "Enter your JWT token. Example: eyJhbGci..."
        });

        // Apply the security requirement globally
        // Every endpoint shows the padlock — protected ones require the token
        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        // Include XML comments in Swagger UI
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    });

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    // Build the App 

    var app = builder.Build();

    // Configure Middleware 

    // Exception middleware first — wraps everything
    app.UseMiddleware<ExceptionMiddleware>();

    // Serilog request logging — logs every HTTP request automatically
    // Placed after exception middleware so failed requests are still logged
    app.UseSerilogRequestLogging(options =>
    {
        // Customize what gets logged per request
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
            options.RoutePrefix = string.Empty;
        });
    }
    app.UseRateLimiter();
    // Health check endpoint — used by hosting platforms to verify the app is running
    app.MapGet("/health", () => Results.Ok(new
    {
        status = "healthy",
        timestamp = DateTime.UtcNow,
        version = "1.0"
    }));
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    // If startup fails completely, log the fatal error
    Log.Fatal(ex, "TaskFlow API failed to start.");
}
finally
{
    // Always flush and close the log on shutdown
    Log.CloseAndFlush();
}