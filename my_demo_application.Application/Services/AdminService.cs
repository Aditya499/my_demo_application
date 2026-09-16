using Microsoft.EntityFrameworkCore;
using my_demo_application.Application.Common;
using my_demo_application.Application.Interfaces;
using my_demo_application.Application.ViewModels;
using my_demo_application.Domain.Entities;
using my_demo_application.Domain.Enums;

namespace my_demo_application.Application.Services;

public class AdminService : IAdminService
{
    private readonly IApplicationDbContext _context;

    public AdminService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardViewModel> GetDashboardStatsAsync()
    {
        var totalDoctors = await _context.Doctors.CountAsync();
        var totalPatients = await _context.Patients.CountAsync();
        var totalAppointments = await _context.Appointments.CountAsync();
        var pendingApts = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending);
        var confirmedApts = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Confirmed);
        var completedApts = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed);

        var revenue = await _context.Appointments
            .Where(a => a.Status == AppointmentStatus.Completed || a.Status == AppointmentStatus.Confirmed)
            .Join(_context.Doctors, a => a.DoctorId, d => d.DoctorId, (a, d) => d.ConsultationFee)
            .SumAsync();

        var recentApts = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime)
            .Take(6)
            .Select(a => new RecentAppointmentViewModel
            {
                AppointmentId = a.AppointmentId,
                PatientName = a.Patient.User.FullName,
                DoctorName = a.Doctor.User.FullName,
                DepartmentName = a.Doctor.Department.DepartmentName,
                AppointmentDate = a.AppointmentDate,
                TimeSlot = $"{a.StartTime:hh\\:mm} - {a.EndTime:hh\\:mm}",
                Status = a.Status.ToString()
            })
            .ToListAsync();

        var deptStats = await _context.Departments
            .Select(d => new DepartmentStatViewModel
            {
                DepartmentName = d.DepartmentName,
                DoctorCount = d.Doctors.Count(),
                AppointmentCount = _context.Appointments.Count(a => a.Doctor.DepartmentId == d.DepartmentId)
            })
            .ToListAsync();

        return new AdminDashboardViewModel
        {
            TotalDoctors = totalDoctors,
            TotalPatients = totalPatients,
            TotalAppointments = totalAppointments,
            PendingAppointments = pendingApts,
            ConfirmedAppointments = confirmedApts,
            CompletedAppointments = completedApts,
            TotalRevenue = revenue,
            RecentAppointments = recentApts,
            DepartmentStats = deptStats
        };
    }

    public async Task<IEnumerable<DoctorCreateEditViewModel>> GetAllDoctorsAsync(string? search = null, int? departmentId = null)
    {
        var query = _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.User.FullName.Contains(search) || d.Specialization.Contains(search));
        }

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(d => d.DepartmentId == departmentId.Value);
        }

        return await query.Select(d => new DoctorCreateEditViewModel
        {
            DoctorId = d.DoctorId,
            FullName = d.User.FullName,
            Email = d.User.Email,
            Phone = d.User.Phone,
            DepartmentId = d.DepartmentId,
            Specialization = d.Specialization,
            Qualification = d.Qualification,
            ExperienceYears = d.ExperienceYears,
            ConsultationFee = d.ConsultationFee,
            Biography = d.Biography,
            IsAvailable = d.IsAvailable
        }).ToListAsync();
    }

    public async Task<ApiResponse<int>> CreateDoctorAsync(DoctorCreateEditViewModel model)
    {
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower()))
        {
            return ApiResponse<int>.Fail("A user with this email address already exists.");
        }

        var password = string.IsNullOrWhiteSpace(model.Password) ? "Doctor@123" : model.Password;
        var user = new User
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Doctor,
            Phone = model.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var doctor = new Doctor
        {
            UserId = user.Id,
            DepartmentId = model.DepartmentId,
            Specialization = model.Specialization,
            Qualification = model.Qualification,
            ExperienceYears = model.ExperienceYears,
            ConsultationFee = model.ConsultationFee,
            Biography = model.Biography,
            IsAvailable = model.IsAvailable
        };

        _context.Doctors.Add(doctor);
        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(doctor.DoctorId, "Doctor created successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateDoctorAsync(DoctorCreateEditViewModel model)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.DoctorId == model.DoctorId);

        if (doctor == null) return ApiResponse<bool>.Fail("Doctor not found.");

        doctor.User.FullName = model.FullName.Trim();
        doctor.User.Phone = model.Phone;
        doctor.User.UpdatedAt = DateTime.UtcNow;

        doctor.DepartmentId = model.DepartmentId;
        doctor.Specialization = model.Specialization;
        doctor.Qualification = model.Qualification;
        doctor.ExperienceYears = model.ExperienceYears;
        doctor.ConsultationFee = model.ConsultationFee;
        doctor.Biography = model.Biography;
        doctor.IsAvailable = model.IsAvailable;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Doctor updated successfully.");
    }

    public async Task<ApiResponse<bool>> ToggleDoctorStatusAsync(int doctorId)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

        if (doctor == null) return ApiResponse<bool>.Fail("Doctor not found.");

        doctor.IsAvailable = !doctor.IsAvailable;
        doctor.User.IsActive = doctor.IsAvailable;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(doctor.IsAvailable, $"Doctor status updated to {(doctor.IsAvailable ? "Active" : "Inactive")}.");
    }

    public async Task<IEnumerable<PatientListItemViewModel>> GetAllPatientsAsync(string? search = null)
    {
        var query = _context.Patients
            .Include(p => p.User)
            .Include(p => p.Appointments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.User.FullName.Contains(search) || p.User.Email.Contains(search));
        }

        return await query.Select(p => new PatientListItemViewModel
        {
            PatientId = p.PatientId,
            UserId = p.UserId,
            FullName = p.User.FullName,
            Email = p.User.Email,
            Phone = p.User.Phone ?? "N/A",
            Gender = p.Gender,
            BloodGroup = p.BloodGroup,
            DateOfBirth = p.DateOfBirth,
            TotalAppointments = p.Appointments.Count,
            IsActive = p.User.IsActive
        }).ToListAsync();
    }

    public async Task<IEnumerable<DepartmentViewModel>> GetAllDepartmentsAsync()
    {
        return await _context.Departments
            .Include(d => d.Doctors)
            .Select(d => new DepartmentViewModel
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                Icon = d.Icon,
                IsActive = d.IsActive,
                DoctorCount = d.Doctors.Count
            }).ToListAsync();
    }

    public async Task<ApiResponse<int>> CreateDepartmentAsync(DepartmentViewModel model)
    {
        var dept = new Department
        {
            DepartmentName = model.DepartmentName.Trim(),
            Description = model.Description ?? string.Empty,
            Icon = string.IsNullOrWhiteSpace(model.Icon) ? "fa-stethoscope" : model.Icon,
            IsActive = true
        };

        _context.Departments.Add(dept);
        await _context.SaveChangesAsync();
        return ApiResponse<int>.Ok(dept.DepartmentId, "Department created successfully.");
    }

    public async Task<ApiResponse<bool>> UpdateDepartmentAsync(DepartmentViewModel model)
    {
        var dept = await _context.Departments.FindAsync(model.DepartmentId);
        if (dept == null) return ApiResponse<bool>.Fail("Department not found.");

        dept.DepartmentName = model.DepartmentName.Trim();
        dept.Description = model.Description ?? string.Empty;
        dept.Icon = model.Icon;
        dept.IsActive = model.IsActive;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Department updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteDepartmentAsync(int departmentId)
    {
        var hasDoctors = await _context.Doctors.AnyAsync(d => d.DepartmentId == departmentId);
        if (hasDoctors)
        {
            return ApiResponse<bool>.Fail("Cannot delete department because doctors are assigned to it.");
        }

        var dept = await _context.Departments.FindAsync(departmentId);
        if (dept == null) return ApiResponse<bool>.Fail("Department not found.");

        _context.Departments.Remove(dept);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Department deleted successfully.");
    }

    public async Task<IEnumerable<RecentAppointmentViewModel>> GetAllAppointmentsAsync(string? status = null, DateTime? date = null)
    {
        var query = _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(a => a.Status == parsedStatus);
        }

        if (date.HasValue)
        {
            query = query.Where(a => a.AppointmentDate.Date == date.Value.Date);
        }

        return await query
            .OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Select(a => new RecentAppointmentViewModel
            {
                AppointmentId = a.AppointmentId,
                PatientName = a.Patient.User.FullName,
                DoctorName = a.Doctor.User.FullName,
                DepartmentName = a.Doctor.Department.DepartmentName,
                AppointmentDate = a.AppointmentDate,
                TimeSlot = $"{a.StartTime:hh\\:mm} - {a.EndTime:hh\\:mm}",
                Status = a.Status.ToString()
            }).ToListAsync();
    }

    public async Task<ApiResponse<bool>> UpdateAppointmentStatusAsync(int appointmentId, string newStatus)
    {
        if (!Enum.TryParse<AppointmentStatus>(newStatus, true, out var status))
        {
            return ApiResponse<bool>.Fail("Invalid appointment status.");
        }

        var apt = await _context.Appointments.FindAsync(appointmentId);
        if (apt == null) return ApiResponse<bool>.Fail("Appointment not found.");

        // Business rules: cannot complete cancelled appointment
        if (apt.Status == AppointmentStatus.Cancelled && status == AppointmentStatus.Completed)
        {
            return ApiResponse<bool>.Fail("A cancelled appointment cannot be marked as completed.");
        }

        // Business rules: cannot cancel completed appointment
        if (apt.Status == AppointmentStatus.Completed && status == AppointmentStatus.Cancelled)
        {
            return ApiResponse<bool>.Fail("A completed appointment cannot be cancelled.");
        }

        apt.Status = status;
        apt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, $"Appointment status changed to {status}.");
    }
}
