using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskFlowAPI.Data;
using TaskFlowAPI.Repositories;
using TaskFlowAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register AppDbContext with SqlServer
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
// Register the repository — when a class asks for ITaskRepository, give it TaskRepository
// Scoped means one instance per HTTP request
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        // validate that the token is issued by our server
        ValidateIssuer=true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        // validate that  the token is intended for our api
        ValidateAudience=true,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        // validate the secret key signature
        ValidateIssuerSigningKey=true,
        IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]!)),
        // validate that the token has not expired
        ValidateLifetime=true,
        // token expires exactly when it says
        ClockSkew=TimeSpan.Zero
    };
});
var app = builder.Build();

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