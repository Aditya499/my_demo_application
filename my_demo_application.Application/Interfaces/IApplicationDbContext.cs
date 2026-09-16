using Microsoft.EntityFrameworkCore;
using my_demo_application.Domain.Entities;

namespace my_demo_application.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<Department> Departments { get; }
    DbSet<Doctor> Doctors { get; }
    DbSet<Patient> Patients { get; }
    DbSet<DoctorAvailability> DoctorAvailabilities { get; }
    DbSet<Appointment> Appointments { get; }
    DbSet<MedicalRecord> MedicalRecords { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
