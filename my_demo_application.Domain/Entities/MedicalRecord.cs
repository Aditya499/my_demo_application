using System.ComponentModel.DataAnnotations;

namespace my_demo_application.Domain.Entities;

public class MedicalRecord
{
    public int MedicalRecordId { get; set; }

    public int AppointmentId { get; set; }
    public virtual Appointment Appointment { get; set; } = null!;

    public int DoctorId { get; set; }
    public virtual Doctor Doctor { get; set; } = null!;

    public int PatientId { get; set; }
    public virtual Patient Patient { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string Diagnosis { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Symptoms { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Prescription { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
