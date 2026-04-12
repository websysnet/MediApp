using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MediApp.Domain.Entities;
using MediApp.Domain.Enums;
using MediApp.Domain.Interfaces;

namespace MediApp.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapGet("/me", GetMe).RequireAuthorization();
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        IUsuarioRepository usuarioRepo)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return Results.BadRequest("Email y contraseña son requeridos");

        if (await usuarioRepo.EmailExistsAsync(request.Email))
            return Results.BadRequest("El email ya está registrado");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var usuario = new Usuario
        {
            Email = request.Email,
            PasswordHash = passwordHash,
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Telefono = request.Telefono,
            Rol = RolUsuario.Paciente,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        await usuarioRepo.AddAsync(usuario);
        await usuarioRepo.SaveChangesAsync();

        return Results.Created($"/api/auth/me", new { usuario.Id, usuario.Email, usuario.Rol });
    }

    private static async Task<IResult> Login(
        [FromBody] LoginRequest request,
        IUsuarioRepository usuarioRepo,
        IConfiguration configuration)
    {
        var usuario = await usuarioRepo.GetByEmailAsync(request.Email);
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            return Results.Unauthorized();

        var jwtKey = configuration["Jwt:Key"] ?? "MediAppSuperSecretKey2026!@#$%^&*()";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "MediApp";
        var jwtAudience = configuration["Jwt:Audience"] ?? "MediApp";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return Results.Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            usuario = new
            {
                usuario.Id,
                usuario.Email,
                usuario.Nombre,
                usuario.Apellido,
                usuario.Rol
            }
        });
    }

    private static async Task<IResult> GetMe(
        [FromServices] IUsuarioRepository usuarioRepo,
        HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var usuario = await usuarioRepo.GetByIdAsync(userId);
        if (usuario == null)
            return Results.NotFound();

        return Results.Ok(new
        {
            usuario.Id,
            usuario.Email,
            usuario.Nombre,
            usuario.Apellido,
            usuario.Telefono,
            usuario.Rol
        });
    }
}

public record RegisterRequest(string Email, string Password, string Nombre, string Apellido, string? Telefono);
public record LoginRequest(string Email, string Password);