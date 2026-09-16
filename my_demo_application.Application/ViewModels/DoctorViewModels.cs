using System.ComponentModel.DataAnnotations;

namespace my_demo_application.Application.ViewModels;

public class DoctorDashboardViewModel
{
    public int DoctorId { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int TodayAppointmentsCount { get; set; }
    public int UpcomingAppointmentsCount { get; set; }
    public int TotalPatientsTreated { get; set; }
    public decimal EstimatedEarnings { get; set; }
    public List<DoctorAppointmentItemViewModel> TodayAppointments { get; set; } = new();
}

public class DoctorAppointmentItemViewModel
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string PatientPhone { get; set; } = string.Empty;
    public string PatientGender { get; set; } = string.Empty;
    public string PatientBloodGroup { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool HasMedicalRecord { get; set; }
}

public class DoctorProfileViewModel
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }

    [Required]
    public string Specialization { get; set; } = string.Empty;

    [Required]
    public string Qualification { get; set; } = string.Empty;

    [Range(0, 60)]
    public int ExperienceYears { get; set; }

    [Range(0, 10000)]
    public decimal ConsultationFee { get; set; }

    public string? Biography { get; set; }
    public bool IsAvailable { get; set; }
}

public class CreateAvailabilitySlotViewModel
{
    [Required]
    [DataType(DataType.Date)]
    public DateTime AvailableDate { get; set; } = DateTime.Today.AddDays(1);

    [Required]
    public TimeSpan StartTime { get; set; } = new TimeSpan(9, 0, 0);

    [Required]
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);

    [Range(15, 120)]
    public int SlotDurationMinutes { get; set; } = 30;
}

public class AddMedicalRecordViewModel
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string Reason { get; set; } = string.Empty;

    [Required(ErrorMessage = "Diagnosis is required")]
    [MaxLength(500)]
    public string Diagnosis { get; set; } = string.Empty;

    [Required(ErrorMessage = "Symptoms description is required")]
    [MaxLength(500)]
    public string Symptoms { get; set; } = string.Empty;

    [Required(ErrorMessage = "Prescription details are required")]
    [MaxLength(2000)]
    public string Prescription { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; }
}
