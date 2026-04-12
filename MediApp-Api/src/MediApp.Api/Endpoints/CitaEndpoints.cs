using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediApp.Domain.Entities;
using MediApp.Domain.Enums;
using MediApp.Domain.Interfaces;

namespace MediApp.Api.Endpoints;

public static class CitaEndpoints
{
    public static void MapCitaEndpoints(this WebApplication app)
    {
        app.MapGet("/api/citas", GetCitas).RequireAuthorization();
        app.MapGet("/api/citas/{id}", GetCita).RequireAuthorization();
        app.MapPost("/api/citas", CreateCita).RequireAuthorization();
        app.MapPut("/api/citas/{id}", UpdateCita).RequireAuthorization();
        app.MapDelete("/api/citas/{id}", CancelCita).RequireAuthorization();
        app.MapGet("/api/citas/disponibilidad", GetDisponibilidad);
        app.MapGet("/api/citas/paciente/{pacienteId}", GetCitasPorPaciente).RequireAuthorization();
        app.MapGet("/api/citas/doctor/{doctorId}", GetCitasPorDoctor).RequireAuthorization();
    }

    private static async Task<IResult> GetCitas(ICitaRepository repo, HttpContext context)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userRole == "Paciente" && int.TryParse(userIdClaim, out var pacienteId))
        {
            var citas = await repo.GetByPacienteIdAsync(pacienteId);
            return Results.Ok(citas.Select(MapCitaToDto));
        }
        
        if (userRole == "Doctor" && int.TryParse(userIdClaim, out var doctorId))
        {
            var citas = await repo.GetByDoctorIdAsync(doctorId);
            return Results.Ok(citas.Select(MapCitaToDto));
        }

        var allCitas = await repo.GetAllAsync();
        return Results.Ok(allCitas.Select(MapCitaToDto));
    }

    private static async Task<IResult> GetCita(int id, ICitaRepository repo, HttpContext context)
    {
        var cita = await repo.GetByIdAsync(id);
        if (cita == null) return Results.NotFound();
        
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        
        if (userRole == "Paciente" && int.TryParse(userIdClaim, out var userId) && cita.PacienteId != userId)
            return Results.Forbid();

        return Results.Ok(MapCitaToDto(cita));
    }

    private static async Task<IResult> GetCitasPorPaciente(int pacienteId, ICitaRepository repo, HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        
        if (userRole == "Paciente" && int.TryParse(userIdClaim, out var userId) && pacienteId != userId)
            return Results.Forbid();

        var citas = await repo.GetByPacienteIdAsync(pacienteId);
        return Results.Ok(citas.Select(MapCitaToDto));
    }

    private static async Task<IResult> GetCitasPorDoctor(int doctorId, ICitaRepository repo)
    {
        var citas = await repo.GetByDoctorIdAsync(doctorId);
        return Results.Ok(citas.Select(MapCitaToDto));
    }

    private static async Task<IResult> CreateCita([FromBody] CreateCitaRequest request, ICitaRepository repo, IDoctorRepository doctorRepo, HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var pacienteId))
            return Results.Unauthorized();

        var doctor = await doctorRepo.GetByIdAsync(request.DoctorId);
        if (doctor == null)
            return Results.BadRequest("Doctor no encontrado");

        if (await repo.ExisteConflicto(request.DoctorId, request.FechaHora))
            return Results.BadRequest("El horario no está disponible");

        var cita = new Cita
        {
            PacienteId = pacienteId,
            DoctorId = request.DoctorId,
            FechaHora = request.FechaHora,
            Motivo = request.Motivo,
            TipoConsulta = request.TipoConsulta,
            Estado = EstadoCita.Pendiente,
            FechaCreacion = DateTime.UtcNow
        };

        await repo.AddAsync(cita);
        await repo.UpdateAsync(cita);

        return Results.Created($"/api/citas/{cita.Id}", MapCitaToDto(cita));
    }

    private static async Task<IResult> UpdateCita(int id, [FromBody] UpdateCitaRequest request, ICitaRepository repo, HttpContext context)
    {
        var cita = await repo.GetByIdAsync(id);
        if (cita == null) return Results.NotFound();

        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userRole == "Paciente" && int.TryParse(userIdClaim, out var userId) && cita.PacienteId != userId)
            return Results.Forbid();

        if (request.Estado.HasValue)
            cita.Estado = request.Estado.Value;
        
        if (!string.IsNullOrEmpty(request.Observaciones))
            cita.Observaciones = request.Observaciones;

        await repo.UpdateAsync(cita);
        return Results.Ok(MapCitaToDto(cita));
    }

    private static async Task<IResult> CancelCita(int id, ICitaRepository repo, HttpContext context)
    {
        var cita = await repo.GetByIdAsync(id);
        if (cita == null) return Results.NotFound();

        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userRole == "Paciente" && int.TryParse(userIdClaim, out var userId) && cita.PacienteId != userId)
            return Results.Forbid();

        if (userRole == "Doctor" && int.TryParse(userIdClaim, out var doctorId) && cita.DoctorId != doctorId)
            return Results.Forbid();

        cita.Estado = EstadoCita.Cancelada;
        await repo.UpdateAsync(cita);
        return Results.NoContent();
    }

    private static async Task<IResult> GetDisponibilidad(int doctorId, DateTime fecha, ICitaRepository repo, IDoctorRepository doctorRepo)
    {
        var doctor = await doctorRepo.GetByIdAsync(doctorId);
        if (doctor == null) return Results.BadRequest("Doctor no encontrado");

        var citasExistentes = await repo.GetByDoctorAndFechaAsync(doctorId, fecha);
        var horariosOcupados = citasExistentes
            .Where(c => c.Estado != EstadoCita.Cancelada)
            .Select(c => c.FechaHora)
            .ToHashSet();

        var horariosDisponibles = new List<string>();
        var duracion = TimeSpan.FromMinutes(doctor.DuracionConsultaMinutos);

        for (var hora = doctor.HorarioInicio; hora <= doctor.HorarioFin - duracion; hora += duracion)
        {
            var fechaHora = fecha.Date + hora;
            if (!horariosOcupados.Contains(fechaHora))
            {
                horariosDisponibles.Add(hora.ToString(@"hh\:mm"));
            }
        }

        return Results.Ok(new
        {
            doctorId,
            fecha = fecha.ToString("yyyy-MM-dd"),
            horarios = horariosDisponibles
        });
    }

    private static object MapCitaToDto(Cita c) => new
    {
        c.Id,
        c.PacienteId,
        Paciente = new { c.Paciente.Nombre, c.Paciente.Apellido },
        c.DoctorId,
        Doctor = new { c.Doctor.Usuario.Nombre, c.Doctor.Usuario.Apellido, c.Doctor.Especialidad },
        c.FechaHora,
        c.Estado,
        c.Motivo,
        c.TipoConsulta,
        c.Observaciones
    };
}

public record CreateCitaRequest(int DoctorId, DateTime FechaHora, string? Motivo, string? TipoConsulta);
public record UpdateCitaRequest(EstadoCita? Estado, string? Observaciones);