using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
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

    // Register Services

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<ITaskRepository, TaskRepository>();
    builder.Services.AddScoped<ITokenService, TokenService>();

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
        app.UseSwaggerUI();
    }

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