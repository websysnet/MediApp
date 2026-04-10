using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MediApp.Domain.Entities;
using MediApp.Domain.Enums;
using MediApp.Domain.Interfaces;
using MediApp.Infrastructure.Data;
using MediApp.Infrastructure.Repositories;
using MediApp.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"] ?? "MediAppSuperSecretKey2026!@#$%^&*()";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MediApp";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MediApp";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("MediAppDB"));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IDoctorRepository, DoctorRepository>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();
builder.Services.AddScoped<IHistoriaClinicaRepository, HistoriaClinicaRepository>();
builder.Services.AddScoped<IRecetaRepository, RecetaRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    
    var userRepo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
    
    if (!await userRepo.EmailExistsAsync("admin@mediapp.com"))
    {
        var adminUser = new Usuario
        {
            Email = "admin@mediapp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Nombre = "Admin",
            Apellido = "Sistema",
            Telefono = "1234567890",
            Rol = RolUsuario.Admin,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };
        await userRepo.AddAsync(adminUser);
        await userRepo.SaveChangesAsync();
        Console.WriteLine("Admin user created: admin@mediapp.com / admin123");
    }
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapDoctorEndpoints();
app.MapCitaEndpoints();
app.MapHistoriaClinicaEndpoints();
app.MapRecetaEndpoints();
app.MapPagoEndpoints();

app.Run();

public partial class Program { }