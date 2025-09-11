using DmsContayPerezIPS.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Minio;
using System;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===== Configuración .env =====
DotNetEnv.Env.Load();

// PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// MinIO
builder.Services.AddSingleton(new MinioClient()
    .WithEndpoint(Environment.GetEnvironmentVariable("MINIO_ENDPOINT") ?? "http://localhost:9000")
    .WithCredentials(
        Environment.GetEnvironmentVariable("MINIO_ACCESS_KEY") ?? "admin",
        Environment.GetEnvironmentVariable("MINIO_SECRET_KEY") ?? "admin123")
    .Build());

// Autenticación JWT
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "ClaveSuperSecreta";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
