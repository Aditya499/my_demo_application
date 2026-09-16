using Microsoft.EntityFrameworkCore;
using my_demo_application.Application.Common;
using my_demo_application.Application.Interfaces;
using my_demo_application.Application.ViewModels;
using my_demo_application.Domain.Entities;
using my_demo_application.Domain.Enums;

namespace my_demo_application.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IApplicationDbContext _context;

    public DoctorService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Doctor?> GetDoctorByUserIdAsync(int userId)
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.UserId == userId);
    }

    public async Task<DoctorDashboardViewModel> GetDashboardAsync(int doctorId)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

        if (doctor == null) throw new KeyNotFoundException("Doctor not found.");

        var today = DateTime.Today;

        var todayApts = await _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.MedicalRecord)
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == today)
            .OrderBy(a => a.StartTime)
            .Select(a => new DoctorAppointmentItemViewModel
            {
                AppointmentId = a.AppointmentId,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                PatientPhone = a.Patient.User.Phone ?? "N/A",
                PatientGender = a.Patient.Gender,
                PatientBloodGroup = a.Patient.BloodGroup,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Reason = a.Reason,
                Status = a.Status.ToString(),
                HasMedicalRecord = a.MedicalRecord != null
            })
            .ToListAsync();

        var upcomingCount = await _context.Appointments
            .CountAsync(a => a.DoctorId == doctorId && a.AppointmentDate > today && a.Status == AppointmentStatus.Confirmed);

        var totalPatientsTreated = await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Completed)
            .Select(a => a.PatientId)
            .Distinct()
            .CountAsync();

        var completedCount = await _context.Appointments
            .CountAsync(a => a.DoctorId == doctorId && a.Status == AppointmentStatus.Completed);

        return new DoctorDashboardViewModel
        {
            DoctorId = doctor.DoctorId,
            DoctorName = doctor.User.FullName,
            Specialization = doctor.Specialization,
            DepartmentName = doctor.Department.DepartmentName,
            TodayAppointmentsCount = todayApts.Count,
            UpcomingAppointmentsCount = upcomingCount,
            TotalPatientsTreated = totalPatientsTreated,
            EstimatedEarnings = completedCount * doctor.ConsultationFee,
            TodayAppointments = todayApts
        };
    }

    public async Task<DoctorProfileViewModel> GetProfileAsync(int doctorId)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

        if (doctor == null) throw new KeyNotFoundException("Doctor not found.");

        return new DoctorProfileViewModel
        {
            DoctorId = doctor.DoctorId,
            FullName = doctor.User.FullName,
            Email = doctor.User.Email,
            Phone = doctor.User.Phone,
            DepartmentName = doctor.Department.DepartmentName,
            DepartmentId = doctor.DepartmentId,
            Specialization = doctor.Specialization,
            Qualification = doctor.Qualification,
            ExperienceYears = doctor.ExperienceYears,
            ConsultationFee = doctor.ConsultationFee,
            Biography = doctor.Biography,
            IsAvailable = doctor.IsAvailable
        };
    }

    public async Task<ApiResponse<bool>> UpdateProfileAsync(DoctorProfileViewModel model)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.DoctorId == model.DoctorId);

        if (doctor == null) return ApiResponse<bool>.Fail("Doctor not found.");

        doctor.User.Phone = model.Phone;
        doctor.Specialization = model.Specialization;
        doctor.Qualification = model.Qualification;
        doctor.ExperienceYears = model.ExperienceYears;
        doctor.ConsultationFee = model.ConsultationFee;
        doctor.Biography = model.Biography;
        doctor.IsAvailable = model.IsAvailable;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Profile updated successfully.");
    }

    public async Task<IEnumerable<DoctorAppointmentItemViewModel>> GetAppointmentsAsync(int doctorId, string? filter = null)
    {
        var today = DateTime.Today;
        var query = _context.Appointments
            .Include(a => a.Patient).ThenInclude(p => p.User)
            .Include(a => a.MedicalRecord)
            .Where(a => a.DoctorId == doctorId)
            .AsQueryable();

        if (filter == "today")
        {
            query = query.Where(a => a.AppointmentDate.Date == today);
        }
        else if (filter == "upcoming")
        {
            query = query.Where(a => a.AppointmentDate.Date >= today && (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending));
        }
        else if (filter == "completed")
        {
            query = query.Where(a => a.Status == AppointmentStatus.Completed);
        }

        return await query
            .OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Select(a => new DoctorAppointmentItemViewModel
            {
                AppointmentId = a.AppointmentId,
                PatientId = a.PatientId,
                PatientName = a.Patient.User.FullName,
                PatientPhone = a.Patient.User.Phone ?? "N/A",
                PatientGender = a.Patient.Gender,
                PatientBloodGroup = a.Patient.BloodGroup,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Reason = a.Reason,
                Status = a.Status.ToString(),
                HasMedicalRecord = a.MedicalRecord != null
            }).ToListAsync();
    }

    public async Task<ApiResponse<bool>> AcceptAppointmentAsync(int appointmentId, int doctorId)
    {
        var apt = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctorId);
        if (apt == null) return ApiResponse<bool>.Fail("Appointment not found.");

        if (apt.Status != AppointmentStatus.Pending)
        {
            return ApiResponse<bool>.Fail($"Only pending appointments can be accepted. Current status: {apt.Status}");
        }

        apt.Status = AppointmentStatus.Confirmed;
        apt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Appointment accepted and confirmed.");
    }

    public async Task<ApiResponse<bool>> RejectAppointmentAsync(int appointmentId, int doctorId)
    {
        var apt = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctorId);
        if (apt == null) return ApiResponse<bool>.Fail("Appointment not found.");

        if (apt.Status == AppointmentStatus.Completed)
        {
            return ApiResponse<bool>.Fail("Completed appointments cannot be rejected.");
        }

        apt.Status = AppointmentStatus.Rejected;
        apt.UpdatedAt = DateTime.UtcNow;

        // Free the availability slot if one exists
        var slot = await _context.DoctorAvailabilities.FirstOrDefaultAsync(da =>
            da.DoctorId == doctorId &&
            da.AvailableDate.Date == apt.AppointmentDate.Date &&
            da.StartTime == apt.StartTime);

        if (slot != null) slot.IsBooked = false;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Appointment rejected.");
    }

    public async Task<ApiResponse<bool>> CompleteAppointmentAsync(int appointmentId, int doctorId)
    {
        var apt = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctorId);
        if (apt == null) return ApiResponse<bool>.Fail("Appointment not found.");

        if (apt.Status == AppointmentStatus.Cancelled || apt.Status == AppointmentStatus.Rejected)
        {
            return ApiResponse<bool>.Fail("Cancelled or rejected appointments cannot be completed.");
        }

        apt.Status = AppointmentStatus.Completed;
        apt.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Appointment marked as completed.");
    }

    public async Task<ApiResponse<bool>> CancelAppointmentAsync(int appointmentId, int doctorId)
    {
        var apt = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.DoctorId == doctorId);
        if (apt == null) return ApiResponse<bool>.Fail("Appointment not found.");

        if (apt.Status == AppointmentStatus.Completed)
        {
            return ApiResponse<bool>.Fail("Completed appointments cannot be cancelled.");
        }

        apt.Status = AppointmentStatus.Cancelled;
        apt.UpdatedAt = DateTime.UtcNow;

        var slot = await _context.DoctorAvailabilities.FirstOrDefaultAsync(da =>
            da.DoctorId == doctorId &&
            da.AvailableDate.Date == apt.AppointmentDate.Date &&
            da.StartTime == apt.StartTime);

        if (slot != null) slot.IsBooked = false;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Appointment cancelled.");
    }

    public async Task<IEnumerable<DoctorAvailability>> GetAvailabilitiesAsync(int doctorId, DateTime? fromDate = null)
    {
        var date = fromDate?.Date ?? DateTime.Today;
        return await _context.DoctorAvailabilities
            .Where(da => da.DoctorId == doctorId && da.AvailableDate.Date >= date)
            .OrderBy(da => da.AvailableDate).ThenBy(da => da.StartTime)
            .ToListAsync();
    }

    public async Task<ApiResponse<int>> GenerateSlotsAsync(int doctorId, CreateAvailabilitySlotViewModel model)
    {
        if (model.AvailableDate.Date < DateTime.Today)
        {
            return ApiResponse<int>.Fail("Cannot create availability slots for past dates.");
        }

        if (model.StartTime >= model.EndTime)
        {
            return ApiResponse<int>.Fail("Start time must be before end time.");
        }

        var duration = TimeSpan.FromMinutes(model.SlotDurationMinutes);
        var currentStart = model.StartTime;
        var slotsCreated = 0;

        while (currentStart + duration <= model.EndTime)
        {
            var currentEnd = currentStart + duration;

            // Check if overlapping slot exists
            bool exists = await _context.DoctorAvailabilities.AnyAsync(da =>
                da.DoctorId == doctorId &&
                da.AvailableDate.Date == model.AvailableDate.Date &&
                ((currentStart >= da.StartTime && currentStart < da.EndTime) ||
                 (currentEnd > da.StartTime && currentEnd <= da.EndTime)));

            if (!exists)
            {
                _context.DoctorAvailabilities.Add(new DoctorAvailability
                {
                    DoctorId = doctorId,
                    AvailableDate = model.AvailableDate.Date,
                    StartTime = currentStart,
                    EndTime = currentEnd,
                    SlotDurationMinutes = model.SlotDurationMinutes,
                    IsBooked = false
                });
                slotsCreated++;
            }

            currentStart = currentEnd;
        }

        await _context.SaveChangesAsync();
        return ApiResponse<int>.Ok(slotsCreated, $"{slotsCreated} slot(s) generated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteSlotAsync(int availabilityId, int doctorId)
    {
        var slot = await _context.DoctorAvailabilities.FirstOrDefaultAsync(da => da.AvailabilityId == availabilityId && da.DoctorId == doctorId);
        if (slot == null) return ApiResponse<bool>.Fail("Slot not found.");

        if (slot.IsBooked)
        {
            return ApiResponse<bool>.Fail("Cannot delete a slot that has an active booking.");
        }

        _context.DoctorAvailabilities.Remove(slot);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Availability slot deleted.");
    }

    public async Task<ApiResponse<int>> AddMedicalRecordAsync(AddMedicalRecordViewModel model, int doctorId)
    {
        var apt = await _context.Appointments
            .Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId && a.DoctorId == doctorId);

        if (apt == null) return ApiResponse<int>.Fail("Appointment not found or not assigned to you.");

        if (apt.MedicalRecord != null)
        {
            return ApiResponse<int>.Fail("Medical record already exists for this appointment.");
        }

        var record = new MedicalRecord
        {
            AppointmentId = apt.AppointmentId,
            DoctorId = doctorId,
            PatientId = apt.PatientId,
            Diagnosis = model.Diagnosis.Trim(),
            Symptoms = model.Symptoms.Trim(),
            Prescription = model.Prescription.Trim(),
            Notes = model.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // Also ensure appointment is marked as completed
        apt.Status = AppointmentStatus.Completed;
        apt.UpdatedAt = DateTime.UtcNow;

        _context.MedicalRecords.Add(record);
        await _context.SaveChangesAsync();

        return ApiResponse<int>.Ok(record.MedicalRecordId, "Medical record created and appointment completed!");
    }

    public async Task<IEnumerable<MedicalRecordItemViewModel>> GetPatientHistoryAsync(int patientId)
    {
        return await _context.MedicalRecords
            .Include(mr => mr.Doctor).ThenInclude(d => d.User)
            .Include(mr => mr.Doctor).ThenInclude(d => d.Department)
            .Include(mr => mr.Appointment)
            .Where(mr => mr.PatientId == patientId)
            .OrderByDescending(mr => mr.CreatedAt)
            .Select(mr => new MedicalRecordItemViewModel
            {
                MedicalRecordId = mr.MedicalRecordId,
                AppointmentId = mr.AppointmentId,
                DoctorName = mr.Doctor.User.FullName,
                Specialization = mr.Doctor.Specialization,
                DepartmentName = mr.Doctor.Department.DepartmentName,
                AppointmentDate = mr.Appointment.AppointmentDate,
                Diagnosis = mr.Diagnosis,
                Symptoms = mr.Symptoms,
                Prescription = mr.Prescription,
                Notes = mr.Notes,
                CreatedAt = mr.CreatedAt
            }).ToListAsync();
    }
}
