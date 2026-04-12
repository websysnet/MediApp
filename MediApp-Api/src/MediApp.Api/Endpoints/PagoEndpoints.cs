using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediApp.Domain.Entities;
using MediApp.Domain.Enums;
using MediApp.Domain.Interfaces;

namespace MediApp.Api.Endpoints;

public static class PagoEndpoints
{
    public static void MapPagoEndpoints(this WebApplication app)
    {
        app.MapGet("/api/pagos", GetPagos).RequireAuthorization();
        app.MapGet("/api/pagos/cita/{citaId}", GetPagoPorCita).RequireAuthorization();
        app.MapPost("/api/pagos", CreatePago).RequireAuthorization();
    }

    private static async Task<IResult> GetPagos(IPagoRepository repo, HttpContext context)
    {
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        
        if (userRole != "Admin" && userRole != "Doctor")
            return Results.Forbid();

        var pagos = await repo.GetAllAsync();
        return Results.Ok(pagos.Select(p => new
        {
            p.Id,
            p.CitaId,
            p.Monto,
            p.Metodo,
            p.Estado,
            p.FechaPago,
            Paciente = new { p.Cita.Paciente.Nombre, p.Cita.Paciente.Apellido }
        }));
    }

    private static async Task<IResult> GetPagoPorCita(int citaId, IPagoRepository repo)
    {
        var pago = await repo.GetByCitaIdAsync(citaId);
        if (pago == null) return Results.NotFound();

        return Results.Ok(new
        {
            pago.Id,
            pago.CitaId,
            pago.Monto,
            pago.Metodo,
            pago.Estado,
            pago.FechaPago
        });
    }

    private static async Task<IResult> CreatePago([FromBody] CreatePagoRequest request, IPagoRepository repo, ICitaRepository citaRepo, HttpContext context)
    {
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Results.Unauthorized();

        var cita = await citaRepo.GetByIdAsync(request.CitaId);
        if (cita == null)
            return Results.BadRequest("Cita no encontrada");

        if (cita.PacienteId != userId)
            return Results.Forbid();

        var pagoExistente = await repo.GetByCitaIdAsync(request.CitaId);
        if (pagoExistente != null)
            return Results.BadRequest("Ya existe un pago para esta cita");

        var pago = new Pago
        {
            CitaId = request.CitaId,
            Monto = request.Monto > 0 ? request.Monto : cita.Doctor.PrecioConsulta,
            Metodo = request.Metodo,
            Estado = EstadoPago.Pendiente,
            FechaCreacion = DateTime.UtcNow
        };

        await repo.AddAsync(pago);
        await repo.UpdateAsync(pago);

        return Results.Created($"/api/pagos/cita/{pago.CitaId}", new
        {
            pago.Id,
            pago.Monto,
            pago.Estado
        });
    }
}

public record CreatePagoRequest(int CitaId, decimal Monto, string Metodo);