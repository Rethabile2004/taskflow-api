using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using TaskFlowAPI.Data;
using TaskFlowAPI.Middleware;
using TaskFlowAPI.Repositories;
using TaskFlowAPI.Services;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("TaskFlow API starting up...");

    var builder = WebApplication.CreateBuilder(args);

    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

    var secretKey = builder.Configuration["JwtSettings:SecretKey"];

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File(
            path: "Logs/taskflow-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
            retainedFileCountLimit: 7)
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information));

    if (string.IsNullOrEmpty(secretKey))
    {
        Log.Fatal("JWT SecretKey missing — set JwtSettings__SecretKey env var on Render.");
        Log.CloseAndFlush();
        throw new InvalidOperationException("JWT SecretKey is missing.");
    }
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.AddFixedWindowLimiter("auth", limiterOptions =>
        {
            limiterOptions.PermitLimit = 5;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

        options.AddFixedWindowLimiter("write", limiterOptions =>
        {
            limiterOptions.PermitLimit = 30;
            limiterOptions.Window = TimeSpan.FromMinutes(1);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;
        });

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
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddSwaggerGen(options =>
    {
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

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token. Example: eyJhbGci..."
        });

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

        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    var rawConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

    var connectionString = rawConn.StartsWith("postgresql://") || rawConn.StartsWith("postgres://")
        ? $"Host={new Uri(rawConn).Host};Database={new Uri(rawConn).AbsolutePath.TrimStart('/')};Username={new Uri(rawConn).UserInfo.Split(':')[0]};Password={new Uri(rawConn).UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true"
        : rawConn;

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<ITaskRepository, TaskRepository>();
    builder.Services.AddScoped<ITokenService, TokenService>();

    var allowedOrigins = builder.Configuration
        .GetSection("AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("ReactAppPolicy", policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
            else
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
        });
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
                         Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

    Log.Information("JWT Key present: {Present}",
        !string.IsNullOrEmpty(builder.Configuration["JwtSettings:SecretKey"]));
    Log.Information("DB connection present: {Present}",
        !string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection")));

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Database migration failed.");
            throw;
        }
    }

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    // Swagger available in all environments — portfolio project
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API v1");
        options.RoutePrefix = string.Empty;
    });

    if (app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseRateLimiter();
    app.UseCors("ReactAppPolicy");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/health", async (AppDbContext db) =>
    {
        var canConnect = await db.Database.CanConnectAsync();

        return Results.Ok(new
        {
            status = canConnect ? "healthy" : "unhealthy",
            timestamp = DateTime.UtcNow
        });
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "TaskFlow API failed to start.");
}
finally
{
    Log.CloseAndFlush();
}