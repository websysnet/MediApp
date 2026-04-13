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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
});

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
    
    var userRepo = scope.ServiceProvider.GetRequiredService<IUsuarioRepository>();
    var doctorRepo = scope.ServiceProvider.GetRequiredService<IDoctorRepository>();
    
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
    
    var doctores = new (string nombre, string apellido, string email, string especialidad)[]
    {
        ("Juan", "Pérez", "juan.perez@mediapp.com", "Cardiología"),
        ("María", "García", "maria.garcia@mediapp.com", "Pediatría"),
        ("Carlos", "Rodríguez", "carlos.rodriguez@mediapp.com", "Dermatología"),
        ("Ana", "López", "ana.lopez@mediapp.com", "Neurología"),
        ("Pedro", "Martínez", "pedro.martinez@mediapp.com", "Ortopedia"),
        ("Laura", "Sánchez", "laura.sanchez@mediapp.com", "Ginecología"),
        ("Miguel", "Torres", "miguel.torres@mediapp.com", "Oftalmología"),
        ("Sofia", "Ramírez", "sofia.ramirez@mediapp.com", "Psicología"),
        ("Diego", "Hernández", "diego.hernandez@mediapp.com", "Urología"),
        ("Carmen", "Jiménez", "carmen.jimenez@mediapp.com", "Endocrinología")
    };
    
    foreach (var (nombre, apellido, email, especialidad) in doctores)
    {
        if (!await userRepo.EmailExistsAsync(email))
        {
            var usuario = new Usuario
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("doctor123"),
                Nombre = nombre,
                Apellido = apellido,
                Telefono = $"555-{new Random().Next(1000, 9999)}",
                Rol = RolUsuario.Doctor,
                FechaCreacion = DateTime.UtcNow,
                Activo = true,
                Doctor = new Doctor
                {
                    Especialidad = especialidad,
                    NumeroLicencia = $"LIC-{new Random().Next(10000, 99999)}",
                    HorarioInicio = new TimeSpan(8, 0, 0),
                    HorarioFin = new TimeSpan(17, 0, 0),
                    DuracionConsultaMinutos = 30,
                    PrecioConsulta = 50.00m + new Random().Next(0, 100)
                }
            };
            await userRepo.AddAsync(usuario);
        }
    }
    await userRepo.SaveChangesAsync();
    Console.WriteLine("10 doctores creados");
    
    var pacientes = new (string nombre, string apellido, string email, string telefono, DateTime fechaNacimiento)[]
    {
        ("Roberto", "Aguilar", "roberto.aguilar@email.com", "555-1234", new DateTime(1985, 3, 15)),
        ("Elena", "Mendoza", "elena.mendoza@email.com", "555-2345", new DateTime(1990, 7, 22)),
        ("Fernando", "Castillo", "fernando.castillo@email.com", "555-3456", new DateTime(1978, 11, 8)),
        ("Isabel", "Ruiz", "isabel.ruiz@email.com", "555-4567", new DateTime(1995, 1, 30)),
        ("Gabriel", "Navarro", "gabriel.navarro@email.com", "555-5678", new DateTime(1982, 5, 12)),
        ("Patricia", "Vega", "patricia.vega@email.com", "555-6789", new DateTime(1988, 9, 25)),
        ("Alberto", "Morales", "alberto.morales@email.com", "555-7890", new DateTime(1975, 12, 3)),
        ("Natalia", "Flores", "natalia.flores@email.com", "555-8901", new DateTime(1992, 4, 18)),
        ("Ricardo", "Reyes", "ricardo.reyes@email.com", "555-9012", new DateTime(1980, 8, 7)),
        ("Veronica", "Herrera", "veronica.herrera@email.com", "555-0123", new DateTime(1998, 2, 14))
    };
    
    foreach (var (nombre, apellido, email, telefono, fechaNacimiento) in pacientes)
    {
        if (!await userRepo.EmailExistsAsync(email))
        {
            var paciente = new Usuario
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("paciente123"),
                Nombre = nombre,
                Apellido = apellido,
                Telefono = telefono,
                Rol = RolUsuario.Paciente,
                FechaNacimiento = fechaNacimiento,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };
            await userRepo.AddAsync(paciente);
        }
    }
    await userRepo.SaveChangesAsync();
    Console.WriteLine("10 pacientes creados");
}

app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

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