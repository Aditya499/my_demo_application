using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using my_demo_application.Domain.Entities;
using my_demo_application.Domain.Enums;

namespace my_demo_application.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(ApplicationDbContext context)
    {
        // Ensure Database is created and migrations applied
        if (context.Database.IsSqlServer())
        {
            await context.Database.MigrateAsync();
        }
        else
        {
            await context.Database.EnsureCreatedAsync();
        }

        if (await context.Users.AnyAsync())
        {
            return; // DB already seeded
        }

        // 1. Departments
        var cardiology = new Department
        {
            DepartmentName = "Cardiology",
            Description = "Comprehensive heart care, cardiovascular diagnostics, and treatments.",
            Icon = "fa-heart-pulse",
            IsActive = true
        };
        var neurology = new Department
        {
            DepartmentName = "Neurology",
            Description = "Brain, spine, and nervous system consultation and advanced therapies.",
            Icon = "fa-brain",
            IsActive = true
        };
        var orthopedics = new Department
        {
            DepartmentName = "Orthopedics",
            Description = "Bone, joint, ligament, and musculoskeletal reconstruction and care.",
            Icon = "fa-bone",
            IsActive = true
        };
        var pediatrics = new Department
        {
            DepartmentName = "Pediatrics",
            Description = "Infant, child, and adolescent wellness, immunizations, and healthcare.",
            Icon = "fa-baby",
            IsActive = true
        };
        var dermatology = new Department
        {
            DepartmentName = "Dermatology",
            Description = "Skin, hair, cosmetic, and dermatological clinical procedures.",
            Icon = "fa-hand-dots",
            IsActive = true
        };

        context.Departments.AddRange(cardiology, neurology, orthopedics, pediatrics, dermatology);
        await context.SaveChangesAsync();

        // 2. Admin User
        var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");
        var adminUser = new User
        {
            FullName = "MedConnect System Admin",
            Email = "admin@medconnect.com",
            PasswordHash = adminPasswordHash,
            Role = UserRole.Admin,
            Phone = "+1 800-555-0100",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);

        // 3. Doctors
        var doctorPasswordHash = BCrypt.Net.BCrypt.HashPassword("Doctor@123");

        var doc1User = new User
        {
            FullName = "Dr. Marcus Smith",
            Email = "dr.smith@medconnect.com",
            PasswordHash = doctorPasswordHash,
            Role = UserRole.Doctor,
            Phone = "+1 800-555-0101",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(doc1User);
        await context.SaveChangesAsync();

        var doctor1 = new Doctor
        {
            UserId = doc1User.Id,
            DepartmentId = cardiology.DepartmentId,
            Specialization = "Interventional Cardiology",
            Qualification = "MD, FACC - Harvard Medical School",
            ExperienceYears = 14,
            ConsultationFee = 120.00m,
            Biography = "Dr. Marcus Smith is a renowned interventional cardiologist specializing in coronary artery disease, preventative heart health, and hypertensive disorders with over 14 years of clinical practice.",
            IsAvailable = true
        };

        var doc2User = new User
        {
            FullName = "Dr. Sarah Johnson",
            Email = "dr.sarah@medconnect.com",
            PasswordHash = doctorPasswordHash,
            Role = UserRole.Doctor,
            Phone = "+1 800-555-0102",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(doc2User);
        await context.SaveChangesAsync();

        var doctor2 = new Doctor
        {
            UserId = doc2User.Id,
            DepartmentId = neurology.DepartmentId,
            Specialization = "Clinical Neurology & Neurotherapeutics",
            Qualification = "MD, DM (Neurology) - Johns Hopkins",
            ExperienceYears = 10,
            ConsultationFee = 150.00m,
            Biography = "Dr. Sarah Johnson is a leading neurologist committed to providing compassionate, evidence-based care for migraines, neuromuscular disorders, and cognitive wellness.",
            IsAvailable = true
        };

        context.Doctors.AddRange(doctor1, doctor2);
        await context.SaveChangesAsync();

        // 4. Patients
        var patientPasswordHash = BCrypt.Net.BCrypt.HashPassword("Patient@123");

        var pat1User = new User
        {
            FullName = "John Doe",
            Email = "john.doe@example.com",
            PasswordHash = patientPasswordHash,
            Role = UserRole.Patient,
            Phone = "+1 555-234-5678",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(pat1User);
        await context.SaveChangesAsync();

        var patient1 = new Patient
        {
            UserId = pat1User.Id,
            DateOfBirth = new DateTime(1990, 5, 14),
            Gender = "Male",
            BloodGroup = "O+",
            Address = "742 Evergreen Terrace, Springfield, OR",
            EmergencyContact = "Jane Doe (+1 555-987-6543)"
        };

        var pat2User = new User
        {
            FullName = "Emily Watson",
            Email = "emily.watson@example.com",
            PasswordHash = patientPasswordHash,
            Role = UserRole.Patient,
            Phone = "+1 555-876-5432",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(pat2User);
        await context.SaveChangesAsync();

        var patient2 = new Patient
        {
            UserId = pat2User.Id,
            DateOfBirth = new DateTime(1995, 8, 22),
            Gender = "Female",
            BloodGroup = "A+",
            Address = "100 Baker Street, London / Seattle, WA",
            EmergencyContact = "Arthur Watson (+1 555-432-1098)"
        };

        context.Patients.AddRange(patient1, patient2);
        await context.SaveChangesAsync();

        // 5. Doctor Availabilities (for today and next 5 days)
        var today = DateTime.Today;
        var availabilities = new List<DoctorAvailability>();

        for (int dayOffset = 0; dayOffset <= 5; dayOffset++)
        {
            var date = today.AddDays(dayOffset);

            // Dr. Smith morning slots (09:00, 10:00, 11:00)
            availabilities.Add(new DoctorAvailability
            {
                DoctorId = doctor1.DoctorId,
                AvailableDate = date,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(9, 30, 0),
                IsBooked = (dayOffset == 0) // today booked
            });
            availabilities.Add(new DoctorAvailability
            {
                DoctorId = doctor1.DoctorId,
                AvailableDate = date,
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                IsBooked = false
            });
            availabilities.Add(new DoctorAvailability
            {
                DoctorId = doctor1.DoctorId,
                AvailableDate = date,
                StartTime = new TimeSpan(11, 0, 0),
                EndTime = new TimeSpan(11, 30, 0),
                IsBooked = false
            });

            // Dr. Sarah afternoon slots (14:00, 15:00, 16:00)
            availabilities.Add(new DoctorAvailability
            {
                DoctorId = doctor2.DoctorId,
                AvailableDate = date,
                StartTime = new TimeSpan(14, 0, 0),
                EndTime = new TimeSpan(14, 30, 0),
                IsBooked = false
            });
            availabilities.Add(new DoctorAvailability
            {
                DoctorId = doctor2.DoctorId,
                AvailableDate = date,
                StartTime = new TimeSpan(15, 0, 0),
                EndTime = new TimeSpan(15, 30, 0),
                IsBooked = false
            });
        }

        context.DoctorAvailabilities.AddRange(availabilities);
        await context.SaveChangesAsync();

        // 6. Sample Appointments
        // Today's confirmed appointment for John Doe with Dr. Marcus Smith
        var apt1 = new Appointment
        {
            DoctorId = doctor1.DoctorId,
            PatientId = patient1.PatientId,
            AppointmentDate = today,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(9, 30, 0),
            Reason = "Routine cardiac checkup and mild chest discomfort after running.",
            Status = AppointmentStatus.Confirmed,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };

        // Completed past appointment for John Doe with Dr. Sarah Johnson
        var apt2 = new Appointment
        {
            DoctorId = doctor2.DoctorId,
            PatientId = patient1.PatientId,
            AppointmentDate = today.AddDays(-7),
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(14, 30, 0),
            Reason = "Persistent tension headaches and sleep disruption.",
            Status = AppointmentStatus.Completed,
            CreatedAt = DateTime.UtcNow.AddDays(-10)
        };

        // Upcoming pending appointment for Emily Watson with Dr. Marcus Smith
        var apt3 = new Appointment
        {
            DoctorId = doctor1.DoctorId,
            PatientId = patient2.PatientId,
            AppointmentDate = today.AddDays(2),
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(10, 30, 0),
            Reason = "Blood pressure evaluation and family history risk assessment.",
            Status = AppointmentStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddHours(-12)
        };

        context.Appointments.AddRange(apt1, apt2, apt3);
        await context.SaveChangesAsync();

        // 7. Medical Record for Completed Appointment (apt2)
        var record1 = new MedicalRecord
        {
            AppointmentId = apt2.AppointmentId,
            DoctorId = doctor2.DoctorId,
            PatientId = patient1.PatientId,
            Diagnosis = "Episodic Tension-Type Headache triggered by screen fatigue and neck strain.",
            Symptoms = "Bilateral dull ache across frontal and occipital regions, sleep onset delay.",
            Prescription = "1. Naproxen Sodium 250mg as needed for acute pain (max 2/day)\n2. Magnesium Glycinate 400mg nightly\n3. Cervical spine ergonomic stretches 15 mins daily",
            Notes = "Patient advised on digital screen breaks (20-20-20 rule) and proper hydration. Follow up if frequency exceeds 3 days/week.",
            CreatedAt = DateTime.UtcNow.AddDays(-7)
        };

        context.MedicalRecords.Add(record1);
        await context.SaveChangesAsync();
    }
}
