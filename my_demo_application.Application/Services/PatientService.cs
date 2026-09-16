using Microsoft.EntityFrameworkCore;
using my_demo_application.Application.Common;
using my_demo_application.Application.Interfaces;
using my_demo_application.Application.ViewModels;
using my_demo_application.Domain.Entities;
using my_demo_application.Domain.Enums;

namespace my_demo_application.Application.Services;

public class PatientService : IPatientService
{
    private readonly IApplicationDbContext _context;

    public PatientService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Patient?> GetPatientByUserIdAsync(int userId)
    {
        return await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<PatientDashboardViewModel> GetDashboardAsync(int patientId)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PatientId == patientId);

        if (patient == null) throw new KeyNotFoundException("Patient not found.");

        var today = DateTime.Today;

        var nextApt = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .Where(a => a.PatientId == patientId && a.AppointmentDate.Date >= today &&
                       (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed))
            .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
            .Select(a => new PatientAppointmentItemViewModel
            {
                AppointmentId = a.AppointmentId,
                DoctorName = a.Doctor.User.FullName,
                Specialization = a.Doctor.Specialization,
                DepartmentName = a.Doctor.Department.DepartmentName,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Reason = a.Reason,
                Status = a.Status.ToString()
            })
            .FirstOrDefaultAsync();

        var upcomingCount = await _context.Appointments
            .CountAsync(a => a.PatientId == patientId && a.AppointmentDate.Date >= today &&
                            (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Confirmed));

        var completedCount = await _context.Appointments
            .CountAsync(a => a.PatientId == patientId && a.Status == AppointmentStatus.Completed);

        var rxCount = await _context.MedicalRecords
            .CountAsync(mr => mr.PatientId == patientId);

        var recentApts = await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .Include(a => a.MedicalRecord)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime)
            .Take(5)
            .Select(a => new PatientAppointmentItemViewModel
            {
                AppointmentId = a.AppointmentId,
                DoctorName = a.Doctor.User.FullName,
                Specialization = a.Doctor.Specialization,
                DepartmentName = a.Doctor.Department.DepartmentName,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Reason = a.Reason,
                Status = a.Status.ToString(),
                MedicalRecordId = a.MedicalRecord != null ? a.MedicalRecord.MedicalRecordId : null
            })
            .ToListAsync();

        var depts = await _context.Departments
            .Include(d => d.Doctors)
            .Where(d => d.IsActive)
            .Take(4)
            .Select(d => new DepartmentCardViewModel
            {
                DepartmentId = d.DepartmentId,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                Icon = d.Icon,
                DoctorCount = d.Doctors.Count(doc => doc.IsAvailable)
            })
            .ToListAsync();

        return new PatientDashboardViewModel
        {
            PatientId = patient.PatientId,
            FullName = patient.User.FullName,
            BloodGroup = patient.BloodGroup,
            UpcomingAppointmentsCount = upcomingCount,
            CompletedAppointmentsCount = completedCount,
            TotalPrescriptionsCount = rxCount,
            NextAppointment = nextApt,
            RecentAppointments = recentApts,
            TopDepartments = depts
        };
    }

    public async Task<PatientProfileViewModel> GetProfileAsync(int patientId)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PatientId == patientId);

        if (patient == null) throw new KeyNotFoundException("Patient not found.");

        return new PatientProfileViewModel
        {
            PatientId = patient.PatientId,
            FullName = patient.User.FullName,
            Email = patient.User.Email,
            Phone = patient.User.Phone ?? string.Empty,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            BloodGroup = patient.BloodGroup,
            Address = patient.Address,
            EmergencyContact = patient.EmergencyContact
        };
    }

    public async Task<ApiResponse<bool>> UpdateProfileAsync(PatientProfileViewModel model)
    {
        var patient = await _context.Patients
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PatientId == model.PatientId);

        if (patient == null) return ApiResponse<bool>.Fail("Patient not found.");

        patient.User.Phone = model.Phone;
        patient.DateOfBirth = model.DateOfBirth;
        patient.Gender = model.Gender;
        patient.BloodGroup = model.BloodGroup;
        patient.Address = model.Address;
        patient.EmergencyContact = model.EmergencyContact;
        patient.User.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Profile updated successfully.");
    }

    public async Task<IEnumerable<DoctorDirectoryItemViewModel>> SearchDoctorsAsync(string? search = null, int? departmentId = null)
    {
        var query = _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .Where(d => d.IsAvailable && d.User.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.User.FullName.Contains(search) || d.Specialization.Contains(search));
        }

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(d => d.DepartmentId == departmentId.Value);
        }

        return await query.Select(d => new DoctorDirectoryItemViewModel
        {
            DoctorId = d.DoctorId,
            FullName = d.User.FullName,
            Specialization = d.Specialization,
            Qualification = d.Qualification,
            DepartmentName = d.Department.DepartmentName,
            ExperienceYears = d.ExperienceYears,
            ConsultationFee = d.ConsultationFee,
            Biography = d.Biography,
            IsAvailable = d.IsAvailable
        }).ToListAsync();
    }

    public async Task<DoctorDirectoryItemViewModel?> GetDoctorDetailsAsync(int doctorId)
    {
        var doctor = await _context.Doctors
            .Include(d => d.User)
            .Include(d => d.Department)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId);

        if (doctor == null) return null;

        return new DoctorDirectoryItemViewModel
        {
            DoctorId = doctor.DoctorId,
            FullName = doctor.User.FullName,
            Specialization = doctor.Specialization,
            Qualification = doctor.Qualification,
            DepartmentName = doctor.Department.DepartmentName,
            ExperienceYears = doctor.ExperienceYears,
            ConsultationFee = doctor.ConsultationFee,
            Biography = doctor.Biography,
            IsAvailable = doctor.IsAvailable
        };
    }

    public async Task<List<SlotItemViewModel>> GetDoctorAvailableSlotsAsync(int doctorId, DateTime date)
    {
        return await _context.DoctorAvailabilities
            .Where(da => da.DoctorId == doctorId && da.AvailableDate.Date == date.Date && !da.IsBooked)
            .OrderBy(da => da.StartTime)
            .Select(da => new SlotItemViewModel
            {
                AvailabilityId = da.AvailabilityId,
                StartTime = da.StartTime,
                EndTime = da.EndTime,
                IsBooked = da.IsBooked
            })
            .ToListAsync();
    }

    public async Task<ApiResponse<int>> BookAppointmentAsync(int patientId, BookAppointmentViewModel model)
    {
        // 1. Validate past date
        if (model.AppointmentDate.Date < DateTime.Today)
        {
            return ApiResponse<int>.Fail("Cannot book an appointment for a past date.");
        }

        // 2. Parse timeslot string e.g. "09:00:00-09:30:00"
        var parts = model.TimeSlot.Split('-');
        if (parts.Length != 2 || !TimeSpan.TryParse(parts[0], out var startTime) || !TimeSpan.TryParse(parts[1], out var endTime))
        {
            return ApiResponse<int>.Fail("Invalid time slot selected.");
        }

        // 3. Prevent double booking: Doctor cannot have another appointment at same date & time
        bool doctorBusy = await _context.Appointments.AnyAsync(a =>
            a.DoctorId == model.DoctorId &&
            a.AppointmentDate.Date == model.AppointmentDate.Date &&
            a.StartTime == startTime &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Rejected);

        if (doctorBusy)
        {
            return ApiResponse<int>.Fail("This doctor already has a booked appointment at the selected date and time.");
        }

        // 4. Prevent double booking: Patient cannot book same doctor or duplicate time slot
        bool patientBusy = await _context.Appointments.AnyAsync(a =>
            a.PatientId == patientId &&
            a.AppointmentDate.Date == model.AppointmentDate.Date &&
            a.StartTime == startTime &&
            a.Status != AppointmentStatus.Cancelled &&
            a.Status != AppointmentStatus.Rejected);

        if (patientBusy)
        {
            return ApiResponse<int>.Fail("You already have an appointment scheduled at this exact time.");
        }

        // 5. Create Appointment
        var appointment = new Appointment
        {
            DoctorId = model.DoctorId,
            PatientId = patientId,
            AppointmentDate = model.AppointmentDate.Date,
            StartTime = startTime,
            EndTime = endTime,
            Reason = model.Reason.Trim(),
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);

        // 6. Mark availability slot as booked if one was defined
        var slot = await _context.DoctorAvailabilities.FirstOrDefaultAsync(da =>
            da.DoctorId == model.DoctorId &&
            da.AvailableDate.Date == model.AppointmentDate.Date &&
            da.StartTime == startTime);

        if (slot != null) slot.IsBooked = true;

        await _context.SaveChangesAsync();
        return ApiResponse<int>.Ok(appointment.AppointmentId, "Appointment booked successfully! It is pending doctor confirmation.");
    }

    public async Task<IEnumerable<PatientAppointmentItemViewModel>> GetMyAppointmentsAsync(int patientId)
    {
        return await _context.Appointments
            .Include(a => a.Doctor).ThenInclude(d => d.User)
            .Include(a => a.Doctor).ThenInclude(d => d.Department)
            .Include(a => a.MedicalRecord)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate).ThenByDescending(a => a.StartTime)
            .Select(a => new PatientAppointmentItemViewModel
            {
                AppointmentId = a.AppointmentId,
                DoctorName = a.Doctor.User.FullName,
                Specialization = a.Doctor.Specialization,
                DepartmentName = a.Doctor.Department.DepartmentName,
                AppointmentDate = a.AppointmentDate,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Reason = a.Reason,
                Status = a.Status.ToString(),
                MedicalRecordId = a.MedicalRecord != null ? a.MedicalRecord.MedicalRecordId : null
            }).ToListAsync();
    }

    public async Task<ApiResponse<bool>> CancelAppointmentAsync(int appointmentId, int patientId)
    {
        var apt = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.PatientId == patientId);
        if (apt == null) return ApiResponse<bool>.Fail("Appointment not found.");

        if (apt.Status == AppointmentStatus.Completed)
        {
            return ApiResponse<bool>.Fail("Completed appointments cannot be cancelled.");
        }

        apt.Status = AppointmentStatus.Cancelled;
        apt.UpdatedAt = DateTime.UtcNow;

        var slot = await _context.DoctorAvailabilities.FirstOrDefaultAsync(da =>
            da.DoctorId == apt.DoctorId &&
            da.AvailableDate.Date == apt.AppointmentDate.Date &&
            da.StartTime == apt.StartTime);

        if (slot != null) slot.IsBooked = false;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Your appointment has been cancelled.");
    }

    public async Task<IEnumerable<MedicalRecordItemViewModel>> GetMyMedicalRecordsAsync(int patientId)
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
