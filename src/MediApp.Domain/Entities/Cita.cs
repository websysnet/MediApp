namespace MediApp.Domain.Entities;

public class Cita
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public int DoctorId { get; set; }
    public DateTime FechaHora { get; set; }
    public Enums.EstadoCita Estado { get; set; } = Enums.EstadoCita.Pendiente;
    public string? Motivo { get; set; }
    public string? TipoConsulta { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Usuario Paciente { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public HistoriaClinica? HistoriaClinica { get; set; }
    public Pago? Pago { get; set; }
}