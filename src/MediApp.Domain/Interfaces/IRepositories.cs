using MediApp.Domain.Entities;

namespace MediApp.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByIdAsync(int id);
    Task<Usuario?> GetByEmailAsync(string email);
    Task<IEnumerable<Usuario>> GetAllAsync();
    Task<IEnumerable<Usuario>> GetPacientesAsync();
    Task AddAsync(Usuario usuario);
    Task UpdateAsync(Usuario usuario);
    Task<bool> EmailExistsAsync(string email);
    Task SaveChangesAsync();
}

public interface IDoctorRepository
{
    Task<Doctor?> GetByIdAsync(int id);
    Task<Doctor?> GetByUsuarioIdAsync(int usuarioId);
    Task<IEnumerable<Doctor>> GetAllAsync();
    Task<IEnumerable<Doctor>> GetActivosAsync();
    Task AddAsync(Doctor doctor);
    Task UpdateAsync(Doctor doctor);
}

public interface ICitaRepository
{
    Task<Cita?> GetByIdAsync(int id);
    Task<IEnumerable<Cita>> GetAllAsync();
    Task<IEnumerable<Cita>> GetByPacienteIdAsync(int pacienteId);
    Task<IEnumerable<Cita>> GetByDoctorIdAsync(int doctorId);
    Task<IEnumerable<Cita>> GetByFechaAsync(DateTime fecha);
    Task<IEnumerable<Cita>> GetByDoctorAndFechaAsync(int doctorId, DateTime fecha);
    Task AddAsync(Cita cita);
    Task UpdateAsync(Cita cita);
    Task<bool> ExisteConflicto(int doctorId, DateTime fechaHora, int? excludeId = null);
}

public interface IHistoriaClinicaRepository
{
    Task<HistoriaClinica?> GetByIdAsync(int id);
    Task<IEnumerable<HistoriaClinica>> GetByPacienteIdAsync(int pacienteId);
    Task AddAsync(HistoriaClinica historia);
    Task UpdateAsync(HistoriaClinica historia);
}

public interface IRecetaRepository
{
    Task<Receta?> GetByIdAsync(int id);
    Task<IEnumerable<Receta>> GetByPacienteIdAsync(int pacienteId);
    Task<IEnumerable<Receta>> GetByHistoriaClinicaIdAsync(int historiaClinicaId);
    Task AddAsync(Receta receta);
    Task UpdateAsync(Receta receta);
}

public interface IPagoRepository
{
    Task<Pago?> GetByIdAsync(int id);
    Task<Pago?> GetByCitaIdAsync(int citaId);
    Task<IEnumerable<Pago>> GetAllAsync();
    Task AddAsync(Pago pago);
    Task UpdateAsync(Pago pago);
}