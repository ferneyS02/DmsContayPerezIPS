// =========================
// Program.cs — DmsContayPerezIPS.API
// =========================

using DmsContayPerezIPS.API.Authorization;          // RBAC: PermissionPolicyProvider (políticas dinámicas "perm:*")
using DmsContayPerezIPS.API.Services;               // ITextExtractor / PdfDocxTextExtractor
using DmsContayPerezIPS.Infrastructure.Persistence; // AppDbContext
using DmsContayPerezIPS.Infrastructure.Seed;        // SeederService.SeedAsync(...)
using DmsContayPerezIPS.Infrastructure.Security;    // IUserPermissionService / UserPermissionService
using DmsContayPerezIPS.Application.Auth;           // PermissionHandler (valida claims "perm")
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;                    // IFormFile (Swagger MapType)
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using Minio.DataModel.Args;
using System.Text;

// ===== Opcional: cargar variables desde .env si existe =====
try { DotNetEnv.Env.Load(); } catch { /* ignore */ }

// ===== Compatibilidad Npgsql para DateTime =====
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ===== PostgreSQL =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== MinIO ===== (usar IMinioClient en vez de MinioClient)
builder.Services.AddSingleton<IMinioClient>(sp =>
{
    var endpoint  = builder.Configuration["MinIO:Endpoint"]  ?? "localhost:9000";
    var accessKey = builder.Configuration["MinIO:AccessKey"] ?? "admin";
    var secretKey = builder.Configuration["MinIO:SecretKey"] ?? "admin123";

    return new MinioClient()
        .WithEndpoint(endpoint)
        .WithCredentials(accessKey, secretKey)
        // .WithSSL() // habilítalo si usas https en MinIO
        .Build();
});

// ===== JWT Authentication =====
var jwtKey =
    builder.Configuration["JWT:Key"]   // convención actual
 ?? builder.Configuration["Jwt:Key"]   // compatibilidad
 ?? "EstaEsUnaClaveJWTDeAlMenos32CaracteresSuperSegura!!123";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var issuer   = builder.Configuration["JWT:Issuer"]   ?? builder.Configuration["Jwt:Issuer"];
        var audience = builder.Configuration["JWT:Audience"] ?? builder.Configuration["Jwt:Audience"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = issuer,
            ValidAudience            = audience,
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// ===== Authorization (RBAC dinámico por permisos) =====
builder.Services.AddAuthorization(); // habilita [Authorize]
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>(); // "perm:<slug>"
builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();                 // valida claim "perm"
builder.Services.AddScoped<IUserPermissionService, UserPermissionService>();            // resuelve permisos desde BD
// builder.Services.AddScoped<DmsContayPerezIPS.API.Auth.JwtFactory>(); // <- opcional si usarás el JwtFactory para emitir tokens con claims "perm"

// ===== Controllers + Swagger =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DmsContayPerezIPS.API",
        Version = "v1"
    });

    // Seguridad JWT en Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.ApiKey,
        Scheme       = "Bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Escribe: Bearer {tu_token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Arreglo para endpoints con IFormFile en multipart/form-data
    c.MapType<IFormFile>(() => new OpenApiSchema
    {
        Type = "string",
        Format = "binary"
    });
});

// ===== Extractor de texto (PDF/DOCX) =====
builder.Services.AddScoped<ITextExtractor, PdfDocxTextExtractor>();

// ===== Build app =====
var app = builder.Build();

// ===== Crear bucket, ejecutar migraciones y seeding =====
using (var scope = app.Services.CreateScope())
{
    var minio  = scope.ServiceProvider.GetRequiredService<IMinioClient>();
    var db     = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var bucket = builder.Configuration["MinIO:Bucket"] ?? "dms";

    try
    {
        bool exists = await minio.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket));
        if (!exists)
        {
            await minio.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket));
            Console.WriteLine($"Bucket '{bucket}' creado en MinIO.");
        }
        else
        {
            Console.WriteLine($"Bucket '{bucket}' ya existe en MinIO.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error verificando/creando bucket en MinIO: {ex.Message}");
    }

    // Migraciones
    await db.Database.MigrateAsync();

    // Seeding (roles, admin, etc.) — usa tu SeederService existente
    await SeederService.SeedAsync(db, minio, bucket);

    // (Opcional) Enlazar automáticamente un admin al rol admin.dms si config "Admin:UserId" está definida:
    // using DmsContayPerezIPS.Infrastructure.Persistence.Seeds;
    // try
    // {
    //     var adminUserIdValue = builder.Configuration["Admin:UserId"];
    //     if (Guid.TryParse(adminUserIdValue, out var adminId))
    //     {
    //         await db.EnsureAdminAsync(adminId); // helper EnsureAdminAsync (si lo agregaste en Infrastructure.Persistence.Seeds)
    //         Console.WriteLine($"Admin vinculado al rol admin.dms: {adminId}");
    //     }
    // }
    // catch { /* ignora si no tienes aún el helper */ }
}

// ===== Middlewares =====
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
