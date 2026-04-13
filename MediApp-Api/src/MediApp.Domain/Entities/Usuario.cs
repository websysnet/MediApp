namespace MediApp.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public Enums.RolUsuario Rol { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public bool Activo { get; set; } = true;

    public Doctor? Doctor { get; set; }
}

public class Doctor
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Especialidad { get; set; } = string.Empty;
    public string NumeroLicencia { get; set; } = string.Empty;
    public TimeSpan HorarioInicio { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan HorarioFin { get; set; } = new TimeSpan(17, 0, 0);
    public int DuracionConsultaMinutos { get; set; } = 30;
    public decimal PrecioConsulta { get; set; } = 50.00m;
    public bool Activo { get; set; } = true;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    public ICollection<HistoriaClinica> HistoriasClinicas { get; set; } = new List<HistoriaClinica>();
}