namespace MediApp.Domain.Entities;

public class HistoriaClinica
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public int DoctorId { get; set; }
    public int? CitaId { get; set; }
    public string Diagnostico { get; set; } = string.Empty;
    public string? Observaciones { get; set; }
    public string? Sintomas { get; set; }
    public string? Alergias { get; set; }
    public string? Antecedentes { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Usuario Paciente { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Cita? Cita { get; set; }
    public ICollection<Receta> Recetas { get; set; } = new List<Receta>();
}

public class Receta
{
    public int Id { get; set; }
    public int HistoriaClinicaId { get; set; }
    public string Medicamentos { get; set; } = string.Empty;
    public string? Instrucciones { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public HistoriaClinica HistoriaClinica { get; set; } = null!;
}

public class Pago
{
    public int Id { get; set; }
    public int CitaId { get; set; }
    public decimal Monto { get; set; }
    public string? Metodo { get; set; }
    public Enums.EstadoPago Estado { get; set; } = Enums.EstadoPago.Pendiente;
    public DateTime? FechaPago { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Cita Cita { get; set; } = null!;
}