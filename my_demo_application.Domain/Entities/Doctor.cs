using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace my_demo_application.Domain.Entities;

public class Doctor
{
    public int DoctorId { get; set; }

    public int UserId { get; set; }
    public virtual User User { get; set; } = null!;

    public int DepartmentId { get; set; }
    public virtual Department Department { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Specialization { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Qualification { get; set; } = string.Empty;

    public int ExperienceYears { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ConsultationFee { get; set; }

    [MaxLength(1000)]
    public string? Biography { get; set; }

    [MaxLength(255)]
    public string? ProfileImage { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Navigation
    public virtual ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public virtual ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();
}
