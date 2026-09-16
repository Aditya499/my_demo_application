using System.ComponentModel.DataAnnotations;

namespace my_demo_application.Domain.Entities;

public class Patient
{
    public int PatientId { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public DateTime DateOfBirth { get; set; }

    [MaxLength(20)]
    public string Gender { get; set; } = string.Empty;

    [MaxLength(20)]
    public string BloodGroup { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(50)]
    public string? EmergencyContact { get; set; }

    // Navigation
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}
