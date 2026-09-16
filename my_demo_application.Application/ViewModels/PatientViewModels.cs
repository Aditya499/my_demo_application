using System.ComponentModel.DataAnnotations;

namespace my_demo_application.Application.ViewModels;

public class PatientDashboardViewModel
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public int UpcomingAppointmentsCount { get; set; }
    public int CompletedAppointmentsCount { get; set; }
    public int TotalPrescriptionsCount { get; set; }
    public PatientAppointmentItemViewModel? NextAppointment { get; set; }
    public List<PatientAppointmentItemViewModel> RecentAppointments { get; set; } = new();
    public List<DepartmentCardViewModel> TopDepartments { get; set; } = new();
}

public class DepartmentCardViewModel
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DoctorCount { get; set; }
}

public class DoctorDirectoryItemViewModel
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string Qualification { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int ExperienceYears { get; set; }
    public decimal ConsultationFee { get; set; }
    public string? Biography { get; set; }
    public bool IsAvailable { get; set; }
}

public class BookAppointmentViewModel
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public decimal ConsultationFee { get; set; }

    [Required(ErrorMessage = "Please select an appointment date")]
    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Please select an appointment time slot")]
    public string TimeSlot { get; set; } = string.Empty; // Format: "09:00:00-09:30:00"

    [Required(ErrorMessage = "Please provide the reason for consultation")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public List<SlotItemViewModel> AvailableSlots { get; set; } = new();
}

public class SlotItemViewModel
{
    public int AvailabilityId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsBooked { get; set; }
    public string DisplayText => $"{StartTime.Hours:D2}:{StartTime.Minutes:D2} - {EndTime.Hours:D2}:{EndTime.Minutes:D2}";
    public string SlotValue => $"{StartTime}-{EndTime}";
}

public class PatientAppointmentItemViewModel
{
    public int AppointmentId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool CanCancel => Status == "Pending" || Status == "Confirmed";
    public int? MedicalRecordId { get; set; }
}

public class MedicalRecordItemViewModel
{
    public int MedicalRecordId { get; set; }
    public int AppointmentId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Diagnosis { get; set; } = string.Empty;
    public string Symptoms { get; set; } = string.Empty;
    public string Prescription { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PatientProfileViewModel
{
    public int PatientId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public string Gender { get; set; } = string.Empty;

    [Required]
    public string BloodGroup { get; set; } = string.Empty;

    public string? Address { get; set; }
    public string? EmergencyContact { get; set; }
}
