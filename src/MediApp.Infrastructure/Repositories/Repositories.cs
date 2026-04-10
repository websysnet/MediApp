using MediApp.Domain.Entities;
using MediApp.Domain.Interfaces;
using MediApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MediApp.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationDbContext _context;

    public UsuarioRepository(ApplicationDbContext context) => _context = context;

    public async Task<Usuario?> GetByIdAsync(int id) =>
        await _context.Usuarios.FindAsync(id);

    public async Task<Usuario?> GetByEmailAsync(string email) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<IEnumerable<Usuario>> GetAllAsync() =>
        await _context.Usuarios.ToListAsync();

    public async Task<IEnumerable<Usuario>> GetPacientesAsync() =>
        await _context.Usuarios.Where(u => u.Rol == Domain.Enums.RolUsuario.Paciente).ToListAsync();

    public async Task AddAsync(Usuario usuario) =>
        await _context.Usuarios.AddAsync(usuario);

    public async Task UpdateAsync(Usuario usuario) =>
        _context.Usuarios.Update(usuario);

    public async Task<bool> EmailExistsAsync(string email) =>
        await _context.Usuarios.AnyAsync(u => u.Email == email);
}

public class DoctorRepository : IDoctorRepository
{
    private readonly ApplicationDbContext _context;

    public DoctorRepository(ApplicationDbContext context) => _context = context;

    public async Task<Doctor?> GetByIdAsync(int id) =>
        await _context.Doctores.Include(d => d.Usuario).FirstOrDefaultAsync(d => d.Id == id);

    public async Task<Doctor?> GetByUsuarioIdAsync(int usuarioId) =>
        await _context.Doctores.Include(d => d.Usuario).FirstOrDefaultAsync(d => d.UsuarioId == usuarioId);

    public async Task<IEnumerable<Doctor>> GetAllAsync() =>
        await _context.Doctores.Include(d => d.Usuario).ToListAsync();

    public async Task<IEnumerable<Doctor>> GetActivosAsync() =>
        await _context.Doctores.Include(d => d.Usuario).Where(d => d.Activo).ToListAsync();

    public async Task AddAsync(Doctor doctor) =>
        await _context.Doctores.AddAsync(doctor);

    public async Task UpdateAsync(Doctor doctor) =>
        _context.Doctores.Update(doctor);
}

public class CitaRepository : ICitaRepository
{
    private readonly ApplicationDbContext _context;

    public CitaRepository(ApplicationDbContext context) => _context = context;

    public async Task<Cita?> GetByIdAsync(int id) =>
        await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Doctor).ThenInclude(d => d.Usuario)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Cita>> GetAllAsync() =>
        await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Doctor).ThenInclude(d => d.Usuario)
            .OrderByDescending(c => c.FechaHora)
            .ToListAsync();

    public async Task<IEnumerable<Cita>> GetByPacienteIdAsync(int pacienteId) =>
        await _context.Citas
            .Include(c => c.Doctor).ThenInclude(d => d.Usuario)
            .Where(c => c.PacienteId == pacienteId)
            .OrderByDescending(c => c.FechaHora)
            .ToListAsync();

    public async Task<IEnumerable<Cita>> GetByDoctorIdAsync(int doctorId) =>
        await _context.Citas
            .Include(c => c.Paciente)
            .Where(c => c.DoctorId == doctorId)
            .OrderByDescending(c => c.FechaHora)
            .ToListAsync();

    public async Task<IEnumerable<Cita>> GetByFechaAsync(DateTime fecha) =>
        await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Doctor).ThenInclude(d => d.Usuario)
            .Where(c => c.FechaHora.Date == fecha.Date)
            .ToListAsync();

    public async Task<IEnumerable<Cita>> GetByDoctorAndFechaAsync(int doctorId, DateTime fecha) =>
        await _context.Citas
            .Where(c => c.DoctorId == doctorId && c.FechaHora.Date == fecha.Date)
            .ToListAsync();

    public async Task AddAsync(Cita cita) =>
        await _context.Citas.AddAsync(cita);

    public async Task UpdateAsync(Cita cita) =>
        _context.Citas.Update(cita);

    public async Task<bool> ExisteConflicto(int doctorId, DateTime fechaHora, int? excludeId = null)
    {
        var inicio = fechaHora;
        var fin = fechaHora.AddMinutes(30);

        return await _context.Citas
            .AnyAsync(c => c.DoctorId == doctorId
                && c.Estado != Domain.Enums.EstadoCita.Cancelada
                && c.Id != excludeId
                && ((c.FechaHora >= inicio && c.FechaHora < fin)
                    || (c.FechaHora.AddMinutes(30) > inicio && c.FechaHora.AddMinutes(30) <= fin)));
    }
}

public class HistoriaClinicaRepository : IHistoriaClinicaRepository
{
    private readonly ApplicationDbContext _context;

    public HistoriaClinicaRepository(ApplicationDbContext context) => _context = context;

    public async Task<HistoriaClinica?> GetByIdAsync(int id) =>
        await _context.HistoriasClinicas
            .Include(h => h.Paciente)
            .Include(h => h.Doctor).ThenInclude(d => d.Usuario)
            .Include(h => h.Recetas)
            .FirstOrDefaultAsync(h => h.Id == id);

    public async Task<IEnumerable<HistoriaClinica>> GetByPacienteIdAsync(int pacienteId) =>
        await _context.HistoriasClinicas
            .Include(h => h.Doctor).ThenInclude(d => d.Usuario)
            .Where(h => h.PacienteId == pacienteId)
            .OrderByDescending(h => h.Fecha)
            .ToListAsync();

    public async Task AddAsync(HistoriaClinica historia) =>
        await _context.HistoriasClinicas.AddAsync(historia);

    public async Task UpdateAsync(HistoriaClinica historia) =>
        _context.HistoriasClinicas.Update(historia);
}

public class RecetaRepository : IRecetaRepository
{
    private readonly ApplicationDbContext _context;

    public RecetaRepository(ApplicationDbContext context) => _context = context;

    public async Task<Receta?> GetByIdAsync(int id) =>
        await _context.Recetas
            .Include(r => r.HistoriaClinica)
            .ThenInclude(h => h!.Paciente)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<IEnumerable<Receta>> GetByPacienteIdAsync(int pacienteId) =>
        await _context.Recetas
            .Include(r => r.HistoriaClinica)
            .ThenInclude(h => h!.Doctor).ThenInclude(d => d.Usuario)
            .Where(r => r.HistoriaClinica!.PacienteId == pacienteId)
            .OrderByDescending(r => r.Fecha)
            .ToListAsync();

    public async Task<IEnumerable<Receta>> GetByHistoriaClinicaIdAsync(int historiaClinicaId) =>
        await _context.Recetas
            .Where(r => r.HistoriaClinicaId == historiaClinicaId)
            .OrderByDescending(r => r.Fecha)
            .ToListAsync();

    public async Task AddAsync(Receta receta) =>
        await _context.Recetas.AddAsync(receta);

    public async Task UpdateAsync(Receta receta) =>
        _context.Recetas.Update(receta);
}

public class PagoRepository : IPagoRepository
{
    private readonly ApplicationDbContext _context;

    public PagoRepository(ApplicationDbContext context) => _context = context;

    public async Task<Pago?> GetByIdAsync(int id) =>
        await _context.Pagos
            .Include(p => p.Cita)
            .ThenInclude(c => c.Paciente)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Pago?> GetByCitaIdAsync(int citaId) =>
        await _context.Pagos.FirstOrDefaultAsync(p => p.CitaId == citaId);

    public async Task<IEnumerable<Pago>> GetAllAsync() =>
        await _context.Pagos
            .Include(p => p.Cita)
            .ThenInclude(c => c.Paciente)
            .OrderByDescending(p => p.FechaCreacion)
            .ToListAsync();

    public async Task AddAsync(Pago pago) =>
        await _context.Pagos.AddAsync(pago);

    public async Task UpdateAsync(Pago pago) =>
        _context.Pagos.Update(pago);
}