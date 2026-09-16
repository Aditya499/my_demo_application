using System.ComponentModel.DataAnnotations;

namespace my_demo_application.Application.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalDoctors { get; set; }
    public int TotalPatients { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int ConfirmedAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<RecentAppointmentViewModel> RecentAppointments { get; set; } = new();
    public List<DepartmentStatViewModel> DepartmentStats { get; set; } = new();
}

public class RecentAppointmentViewModel
{
    public int AppointmentId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime AppointmentDate { get; set; }
    public string TimeSlot { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class DepartmentStatViewModel
{
    public string DepartmentName { get; set; } = string.Empty;
    public int DoctorCount { get; set; }
    public int AppointmentCount { get; set; }
}

public class DoctorCreateEditViewModel
{
    public int DoctorId { get; set; }

    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; } // required on create, optional on edit

    public string? Phone { get; set; }

    [Required]
    public int DepartmentId { get; set; }

    [Required]
    public string Specialization { get; set; } = string.Empty;

    [Required]
    public string Qualification { get; set; } = string.Empty;

    [Range(0, 70)]
    public int ExperienceYears { get; set; }

    [Range(0, 10000)]
    public decimal ConsultationFee { get; set; }

    public string? Biography { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public class DepartmentViewModel
{
    public int DepartmentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public string? Icon { get; set; }
    public bool IsActive { get; set; } = true;
    public int DoctorCount { get; set; }
}

public class PatientListItemViewModel
{
    public int PatientId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string BloodGroup { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int TotalAppointments { get; set; }
    public bool IsActive { get; set; }
}
