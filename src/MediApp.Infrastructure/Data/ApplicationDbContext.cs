using Microsoft.EntityFrameworkCore;
using MediApp.Domain.Entities;
using MediApp.Domain.Enums;

namespace MediApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Doctor> Doctores => Set<Doctor>();
    public DbSet<Cita> Citas => Set<Cita>();
    public DbSet<HistoriaClinica> HistoriasClinicas => Set<HistoriaClinica>();
    public DbSet<Receta> Recetas => Set<Receta>();
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellido).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Rol).HasConversion<int>();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Doctor>(entity =>
        {
            entity.ToTable("Doctores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Especialidad).IsRequired().HasMaxLength(100);
            entity.Property(e => e.NumeroLicencia).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PrecioConsulta).HasPrecision(10, 2);
            entity.HasOne(e => e.Usuario)
                .WithOne(u => u.Doctor)
                .HasForeignKey<Doctor>(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cita>(entity =>
        {
            entity.ToTable("Citas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Motivo).HasMaxLength(500);
            entity.Property(e => e.TipoConsulta).HasMaxLength(100);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            entity.Property(e => e.Estado).HasConversion<int>();
            entity.HasOne(e => e.Paciente)
                .WithMany()
                .HasForeignKey(e => e.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Doctor)
                .WithMany(d => d.Citas)
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => new { e.DoctorId, e.FechaHora });
        });

        modelBuilder.Entity<HistoriaClinica>(entity =>
        {
            entity.ToTable("HistoriasClinicas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Diagnostico).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Observaciones).HasMaxLength(2000);
            entity.Property(e => e.Sintomas).HasMaxLength(2000);
            entity.Property(e => e.Alergias).HasMaxLength(500);
            entity.Property(e => e.Antecedentes).HasMaxLength(2000);
            entity.HasOne(e => e.Paciente)
                .WithMany()
                .HasForeignKey(e => e.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Doctor)
                .WithMany(d => d.HistoriasClinicas)
                .HasForeignKey(e => e.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Cita)
                .WithOne(c => c.HistoriaClinica)
                .HasForeignKey<HistoriaClinica>(e => e.CitaId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Receta>(entity =>
        {
            entity.ToTable("Recetas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Medicamentos).IsRequired();
            entity.Property(e => e.Instrucciones).HasMaxLength(1000);
            entity.HasOne(e => e.HistoriaClinica)
                .WithMany(h => h.Recetas)
                .HasForeignKey(e => e.HistoriaClinicaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("Pagos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Monto).HasPrecision(10, 2);
            entity.Property(e => e.Metodo).HasMaxLength(50);
            entity.Property(e => e.Estado).HasConversion<int>();
            entity.HasOne(e => e.Cita)
                .WithOne(c => c.Pago)
                .HasForeignKey<Pago>(e => e.CitaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}