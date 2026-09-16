using System.ComponentModel.DataAnnotations;
using my_demo_application.Domain.Enums;

namespace my_demo_application.Domain.Entities;

public class Appointment
{
    public int AppointmentId { get; set; }

    public int DoctorId { get; set; }
    public virtual Doctor Doctor { get; set; } = null!;

    public int PatientId { get; set; }
    public virtual Patient Patient { get; set; } = null!;

    [DataType(DataType.Date)]
    public DateTime AppointmentDate { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    [Required]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual MedicalRecord? MedicalRecord { get; set; }
}
