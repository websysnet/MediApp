using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediApp.Domain.Entities;
using MediApp.Domain.Enums;
using MediApp.Domain.Interfaces;

namespace MediApp.Api.Endpoints;

public static class HistoriaClinicaEndpoints
{
    public static void MapHistoriaClinicaEndpoints(this WebApplication app)
    {
        app.MapGet("/api/historias/paciente/{pacienteId}", GetHistoriasPorPaciente).RequireAuthorization();
        app.MapGet("/api/historias/{id}", GetHistoria).RequireAuthorization();
        app.MapPost("/api/historias", CreateHistoria).RequireAuthorization("Doctor", "Admin");
        app.MapPut("/api/historias/{id}", UpdateHistoria).RequireAuthorization("Doctor");
    }

    private static async Task<IResult> GetHistoriasPorPaciente(int pacienteId, IHistoriaClinicaRepository repo, HttpContext context)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userRole == "Paciente" && int.TryParse(userIdClaim, out var userId) && pacienteId != userId)
            return Results.Forbid();

        var historias = await repo.GetByPacienteIdAsync(pacienteId);
        return Results.Ok(historias.Select(h => new
        {
            h.Id,
            h.PacienteId,
            h.DoctorId,
            Doctor = new { h.Doctor.Usuario.Nombre, h.Doctor.Usuario.Apellido, h.Doctor.Especialidad },
            h.Diagnostico,
            h.Observaciones,
            h.Sintomas,
            h.Alergias,
            h.Antecedentes,
            h.Fecha,
            Recetas = h.Recetas.Select(r => new { r.Id, r.Medicamentos, r.Fecha })
        }));
    }

    private static async Task<IResult> GetHistoria(int id, IHistoriaClinicaRepository repo, HttpContext context)
    {
        var historia = await repo.GetByIdAsync(id);
        if (historia == null) return Results.NotFound();

        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userRole == "Paciente" && int.TryParse(userIdClaim, out var userId) && historia.PacienteId != userId)
            return Results.Forbid();

        return Results.Ok(new
        {
            historia.Id,
            historia.PacienteId,
            historia.DoctorId,
            Doctor = new { historia.Doctor.Usuario.Nombre, historia.Doctor.Usuario.Apellido },
            historia.Diagnostico,
            historia.Observaciones,
            historia.Sintomas,
            historia.Alergias,
            historia.Antecedentes,
            historia.Fecha,
            Recetas = historia.Recetas.Select(r => new { r.Id, r.Medicamentos, r.Instrucciones, r.Fecha })
        });
    }

    private static async Task<IResult> CreateHistoria([FromBody] CreateHistoriaRequest request, IHistoriaClinicaRepository repo, HttpContext context)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Doctor" && userRole != "Admin")
            return Results.Forbid();

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var doctorId))
            return Results.Unauthorized();

        var historia = new HistoriaClinica
        {
            PacienteId = request.PacienteId,
            DoctorId = doctorId,
            CitaId = request.CitaId,
            Diagnostico = request.Diagnostico,
            Observaciones = request.Observaciones,
            Sintomas = request.Sintomas,
            Alergias = request.Alergias,
            Antecedentes = request.Antecedentes,
            Fecha = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow
        };

        await repo.AddAsync(historia);
        await repo.UpdateAsync(historia);

        return Results.Created($"/api/historias/{historia.Id}", new { historia.Id, historia.Diagnostico });
    }

    private static async Task<IResult> UpdateHistoria(int id, [FromBody] UpdateHistoriaRequest request, IHistoriaClinicaRepository repo, HttpContext context)
    {
        var historia = await repo.GetByIdAsync(id);
        if (historia == null) return Results.NotFound();

        if (!string.IsNullOrEmpty(request.Diagnostico))
            historia.Diagnostico = request.Diagnostico;
        if (request.Observaciones != null)
            historia.Observaciones = request.Observaciones;
        if (request.Sintomas != null)
            historia.Sintomas = request.Sintomas;
        if (request.Alergias != null)
            historia.Alergias = request.Alergias;
        if (request.Antecedentes != null)
            historia.Antecedentes = request.Antecedentes;

        await repo.UpdateAsync(historia);
        return Results.Ok(new { historia.Id });
    }
}

public record CreateHistoriaRequest(int PacienteId, int? CitaId, string Diagnostico, string? Observaciones, string? Sintomas, string? Alergias, string? Antecedentes);
public record UpdateHistoriaRequest(string? Diagnostico, string? Observaciones, string? Sintomas, string? Alergias, string? Antecedentes);