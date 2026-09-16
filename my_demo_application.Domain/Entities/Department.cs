using System.ComponentModel.DataAnnotations;

namespace my_demo_application.Domain.Entities;

public class Department
{
    public int DepartmentId { get; set; }

    [Required]
    [MaxLength(100)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Icon { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
}
