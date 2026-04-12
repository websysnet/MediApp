using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediApp.Domain.Entities;
using MediApp.Domain.Enums;
using MediApp.Domain.Interfaces;

using PasswordHasher = BCrypt.Net.BCrypt;

namespace MediApp.Api.Endpoints;

public static class DoctorEndpoints
{
    public static void MapDoctorEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/doctores").RequireAuthorization();

        group.MapGet("/", GetDoctores);
        group.MapGet("/{id}", GetDoctor);
        group.MapPost("/", CreateDoctor).RequireAuthorization("Admin");
        group.MapPut("/{id}", UpdateDoctor).RequireAuthorization("Admin");
        group.MapDelete("/{id}", DeleteDoctor).RequireAuthorization("Admin");
        group.MapPut("/{id}/horario", UpdateHorario);
    }

    private static async Task<IResult> GetDoctores(IDoctorRepository repo)
    {
        var doctores = await repo.GetAllAsync();
        return Results.Ok(doctores.Select(d => new
        {
            d.Id,
            d.UsuarioId,
            d.Especialidad,
            d.NumeroLicencia,
            d.HorarioInicio,
            d.HorarioFin,
            d.DuracionConsultaMinutos,
            d.PrecioConsulta,
            Usuario = new { d.Usuario.Nombre, d.Usuario.Apellido, d.Usuario.Email }
        }));
    }

    private static async Task<IResult> GetDoctor(int id, IDoctorRepository repo)
    {
        var doctor = await repo.GetByIdAsync(id);
        if (doctor == null) return Results.NotFound();
        return Results.Ok(new
        {
            doctor.Id,
            doctor.UsuarioId,
            doctor.Especialidad,
            doctor.NumeroLicencia,
            doctor.HorarioInicio,
            doctor.HorarioFin,
            doctor.DuracionConsultaMinutos,
            doctor.PrecioConsulta,
            Usuario = new { doctor.Usuario.Nombre, doctor.Usuario.Apellido, doctor.Usuario.Email }
        });
    }

    private static async Task<IResult> CreateDoctor(
        [FromBody] CreateDoctorRequest request,
        IUsuarioRepository usuarioRepo,
        IDoctorRepository doctorRepo,
        HttpContext context)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin") return Results.Forbid();

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
            Rol = RolUsuario.Doctor,
            FechaCreacion = DateTime.UtcNow,
            Activo = true
        };

        await usuarioRepo.AddAsync(usuario);
        await usuarioRepo.SaveChangesAsync();

        var doctor = new Doctor
        {
            UsuarioId = usuario.Id,
            Especialidad = request.Especialidad,
            NumeroLicencia = request.NumeroLicencia,
            HorarioInicio = TimeSpan.Parse(request.HorarioInicio),
            HorarioFin = TimeSpan.Parse(request.HorarioFin),
            DuracionConsultaMinutos = request.DuracionConsultaMinutos,
            PrecioConsulta = request.PrecioConsulta
        };

        await doctorRepo.AddAsync(doctor);
        await doctorRepo.SaveChangesAsync();

        return Results.Created($"/api/doctores/{doctor.Id}", new { doctor.Id, doctor.Especialidad });
    }

    private static async Task<IResult> UpdateDoctor(
        int id,
        [FromBody] UpdateDoctorRequest request,
        IDoctorRepository repo,
        HttpContext context)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin") return Results.Forbid();

        var doctor = await repo.GetByIdAsync(id);
        if (doctor == null) return Results.NotFound();

        doctor.Especialidad = request.Especialidad;
        doctor.NumeroLicencia = request.NumeroLicencia;
        if (!string.IsNullOrEmpty(request.HorarioInicio))
            doctor.HorarioInicio = TimeSpan.Parse(request.HorarioInicio);
        if (!string.IsNullOrEmpty(request.HorarioFin))
            doctor.HorarioFin = TimeSpan.Parse(request.HorarioFin);
        doctor.DuracionConsultaMinutos = request.DuracionConsultaMinutos > 0 ? request.DuracionConsultaMinutos : doctor.DuracionConsultaMinutos;
        doctor.PrecioConsulta = request.PrecioConsulta > 0 ? request.PrecioConsulta : doctor.PrecioConsulta;

        await repo.UpdateAsync(doctor);
        return Results.Ok(new { doctor.Id, doctor.Especialidad });
    }

    private static async Task<IResult> DeleteDoctor(int id, IDoctorRepository repo, HttpContext context)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Admin") return Results.Forbid();

        var doctor = await repo.GetByIdAsync(id);
        if (doctor == null) return Results.NotFound();

        doctor.Activo = false;
        await repo.UpdateAsync(doctor);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateHorario(
        int id,
        [FromBody] UpdateHorarioRequest request,
        IDoctorRepository repo)
    {
        var doctor = await repo.GetByIdAsync(id);
        if (doctor == null) return Results.NotFound();

        if (!string.IsNullOrEmpty(request.HorarioInicio))
            doctor.HorarioInicio = TimeSpan.Parse(request.HorarioInicio);
        if (!string.IsNullOrEmpty(request.HorarioFin))
            doctor.HorarioFin = TimeSpan.Parse(request.HorarioFin);
        if (request.DuracionConsultaMinutos > 0)
            doctor.DuracionConsultaMinutos = request.DuracionConsultaMinutos;

        await repo.UpdateAsync(doctor);
        return Results.Ok(new { doctor.Id, doctor.HorarioInicio, doctor.HorarioFin });
    }
}

public record CreateDoctorRequest(string Email, string Password, string Nombre, string Apellido, string? Telefono, string Especialidad, string NumeroLicencia, string HorarioInicio, string HorarioFin, int DuracionConsultaMinutos, decimal PrecioConsulta);
public record UpdateDoctorRequest(string Especialidad, string NumeroLicencia, string? HorarioInicio, string? HorarioFin, int DuracionConsultaMinutos, decimal PrecioConsulta);
public record UpdateHorarioRequest(string? HorarioInicio, string? HorarioFin, int DuracionConsultaMinutos);