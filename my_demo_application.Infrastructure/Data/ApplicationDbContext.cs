using Microsoft.EntityFrameworkCore;
using my_demo_application.Domain.Entities;
using my_demo_application.Application.Interfaces;

namespace my_demo_application.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Primary Keys
        modelBuilder.Entity<Department>().HasKey(d => d.DepartmentId);
        modelBuilder.Entity<Doctor>().HasKey(d => d.DoctorId);
        modelBuilder.Entity<Patient>().HasKey(p => p.PatientId);
        modelBuilder.Entity<DoctorAvailability>().HasKey(da => da.AvailabilityId);
        modelBuilder.Entity<Appointment>().HasKey(a => a.AppointmentId);
        modelBuilder.Entity<MedicalRecord>().HasKey(mr => mr.MedicalRecordId);


        // Unique User Email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // User -> Doctor (1-to-1)
        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.User)
            .WithOne(u => u.Doctor)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User -> Patient (1-to-1)
        modelBuilder.Entity<Patient>()
            .HasOne(p => p.User)
            .WithOne(u => u.Patient)
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Doctor -> Department (Many-to-1)
        modelBuilder.Entity<Doctor>()
            .HasOne(d => d.Department)
            .WithMany(dept => dept.Doctors)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // Doctor -> Availability (1-to-Many)
        modelBuilder.Entity<DoctorAvailability>()
            .HasOne(da => da.Doctor)
            .WithMany(d => d.Availabilities)
            .HasForeignKey(da => da.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on availability
        modelBuilder.Entity<DoctorAvailability>()
            .HasIndex(da => new { da.DoctorId, da.AvailableDate });

        // Appointment -> Doctor
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appointment -> Patient
        modelBuilder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Appointment -> MedicalRecord (1-to-1)
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(mr => mr.Appointment)
            .WithOne(a => a.MedicalRecord)
            .HasForeignKey<MedicalRecord>(mr => mr.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // MedicalRecord -> Doctor & Patient
        modelBuilder.Entity<MedicalRecord>()
            .HasOne(mr => mr.Doctor)
            .WithMany(d => d.MedicalRecords)
            .HasForeignKey(mr => mr.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MedicalRecord>()
            .HasOne(mr => mr.Patient)
            .WithMany(p => p.MedicalRecords)
            .HasForeignKey(mr => mr.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
