using System.ComponentModel.DataAnnotations;

namespace my_demo_application.Domain.Entities;

public class DoctorAvailability
{
    [Key]
    public int AvailabilityId { get; set; }

    public int DoctorId { get; set; }
    public virtual Doctor Doctor { get; set; } = null!;

    [DataType(DataType.Date)]
    public DateTime AvailableDate { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    public int SlotDurationMinutes { get; set; } = 30;

    public bool IsBooked { get; set; } = false;
}
