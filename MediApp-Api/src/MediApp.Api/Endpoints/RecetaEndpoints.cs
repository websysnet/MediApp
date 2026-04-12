using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediApp.Domain.Entities;
using MediApp.Domain.Interfaces;

namespace MediApp.Api.Endpoints;

public static class RecetaEndpoints
{
    public static void MapRecetaEndpoints(this WebApplication app)
    {
        app.MapGet("/api/recetas/paciente/{pacienteId}", GetRecetasPorPaciente).RequireAuthorization();
        app.MapGet("/api/recetas/{id}", GetReceta).RequireAuthorization();
        app.MapPost("/api/recetas", CreateReceta).RequireAuthorization("Doctor", "Admin");
        app.MapGet("/api/recetas/{id}/pdf", DescargarPdf).RequireAuthorization();
    }

    private static async Task<IResult> GetRecetasPorPaciente(int pacienteId, IRecetaRepository repo, HttpContext context)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userRole == "Paciente" && int.TryParse(userIdClaim, out var userId) && pacienteId != userId)
            return Results.Forbid();

        var recetas = await repo.GetByPacienteIdAsync(pacienteId);
        return Results.Ok(recetas.Select(r => new
        {
            r.Id,
            r.HistoriaClinicaId,
            r.Medicamentos,
            r.Instrucciones,
            r.Fecha,
            Doctor = new { r.HistoriaClinica?.Doctor?.Usuario?.Nombre, r.HistoriaClinica?.Doctor?.Usuario?.Apellido }
        }));
    }

    private static async Task<IResult> GetReceta(int id, IRecetaRepository repo, HttpContext context)
    {
        var receta = await repo.GetByIdAsync(id);
        if (receta == null) return Results.NotFound();

        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userRole == "Paciente" && int.TryParse(userIdClaim, out var userId) && receta.HistoriaClinica?.PacienteId != userId)
            return Results.Forbid();

        return Results.Ok(new
        {
            receta.Id,
            receta.HistoriaClinicaId,
            receta.Medicamentos,
            receta.Instrucciones,
            receta.Fecha
        });
    }

    private static async Task<IResult> CreateReceta([FromBody] CreateRecetaRequest request, IRecetaRepository repo, HttpContext context)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        if (userRole != "Doctor" && userRole != "Admin")
            return Results.Forbid();

        var receta = new Receta
        {
            HistoriaClinicaId = request.HistoriaClinicaId,
            Medicamentos = request.Medicamentos,
            Instrucciones = request.Instrucciones,
            Fecha = DateTime.UtcNow,
            FechaCreacion = DateTime.UtcNow
        };

        await repo.AddAsync(receta);
        await repo.UpdateAsync(receta);

        return Results.Created($"/api/recetas/{receta.Id}", new { receta.Id, receta.Medicamentos });
    }

    private static async Task<IResult> DescargarPdf(int id, IRecetaRepository repo)
    {
        var receta = await repo.GetByIdAsync(id);
        if (receta == null) return Results.NotFound();

        var historia = await repo.GetByIdAsync(id);
        var paciente = historia?.HistoriaClinica?.Paciente;
        var doctor = historia?.HistoriaClinica?.Doctor;

        var pdfBytes = GenerarPdfReceta(receta, paciente!, doctor!);

        return Results.File(pdfBytes, "application/pdf", $"receta_{id}.pdf");
    }

    private static byte[] GenerarPdfReceta(Receta receta, Usuario paciente, Doctor doctor)
    {
        var contenido = $@"
========================================
        RECETA MÉDICA
========================================

Fecha: {receta.Fecha:dd/MM/yyyy}

Paciente: {paciente.Nombre} {paciente.Apellido}
Doctor: {doctor.Usuario.Nombre} {doctor.Usuario.Apellido}
Especialidad: {doctor.Especialidad}

----------------------------------------
MEDICAMENTOS:
----------------------------------------
{receta.Medicamentos}

----------------------------------------
INSTRUCCIONES:
----------------------------------------
{receta.Instrucciones ?? "Seguir indicaciones del médico"}

========================================
        FIRMA DEL MÉDICO
========================================
";

        return System.Text.Encoding.UTF8.GetBytes(contenido);
    }
}

public record CreateRecetaRequest(int HistoriaClinicaId, string Medicamentos, string? Instrucciones);