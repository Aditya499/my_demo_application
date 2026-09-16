# 🏥 MedConnect - Healthcare Appointment & Clinical Management System

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-purple?logo=nuget&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC292B?logo=microsoft-sql-server&logoColor=white)
![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20%2F%20Layered-blue)
![Authentication](https://img.shields.io/badge/Auth-Cookie%20%2B%20JWT%20Bearer-green)
![Bootstrap 5](https://img.shields.io/badge/UI-Bootstrap%205%20%2B%20FontAwesome%206-7952B3?logo=bootstrap&logoColor=white)

**MedConnect** is a full-stack, enterprise-grade healthcare appointment and medical record management system designed for **production portfolio demonstrations**, **MCA fresher interviews**, and **.NET Full Stack developer technical screenings**.

It features a modular **Clean Architecture** backend, **Microsoft SQL Server** relational persistence via **Entity Framework Core Code-First**, dual authentication (**Cookie Authentication** for responsive MVC web portals and **JWT Bearer Authentication** for mobile/SPA REST APIs), and dedicated role-based portals for **Admin**, **Doctor**, and **Patient**.

---

## 🌟 Key Modules & Features

### 1. 🛡️ Admin Portal (`/Admin`)
- **Executive Analytics Dashboard**: Real-time KPI cards for total clinicians, registered patients, appointment volume, and total realized revenue.
- **Doctor Directory Management**: Full CRUD for onboarding doctors, assigning specialties/departments, configuring consultation fees, and deactivating accounts.
- **Patient Registry**: Search and view patient profiles, age, contact information, blood group, and total appointment volume.
- **Clinical Departments**: Manage hospital departments (Cardiology, Neurology, Orthopedics, Pediatrics, Dermatology).
- **Master Appointment Control**: Review all patient bookings with status filters (Pending, Confirmed, Completed, Cancelled) and manual status override.
- **Financial & Utilization Reports**: Departmental booking volume distribution and revenue breakdown.

### 2. 🩺 Doctor Portal (`/Doctor`)
- **Clinical Dashboard**: Overview of today's appointment schedule, pending consultation requests, patient volume treated, and estimated earnings.
- **Availability Slot Generator**: Interactive tool to generate non-overlapping daily consultation time slots (15, 30, 45, 60 min intervals).
- **Consultation Workflow**: Accept pending bookings, reject unsuitable slots, complete visits, or cancel appointments.
- **Electronic Health Records (EHR)**: Enter clinical diagnoses, symptoms/findings, and multi-line prescriptions linked directly to completed consultations.
- **Patient Medical History**: Instant access to previous clinical notes, diagnoses, and prescriptions for any assigned patient.
- **Doctor Profile**: Edit credentials, biography, consultation fee, and active acceptance status.

### 3. 👤 Patient Portal (`/Patient`)
- **Patient Dashboard**: Personalized welcome banner, next upcoming appointment alert, and recent prescription quick links.
- **Specialist Discovery**: Search doctors by name or specialty, filter by department, and review credentials and consultation fees.
- **Conflict-Free Slot Booking**: Real-time availability picker that dynamically loads open slots for any chosen date and prevents double-booking.
- **My Appointments**: Comprehensive appointment tracker with real-time status badges and instant cancellation capability for pending/confirmed visits.
- **Digital Medical Records & Prescriptions**: View clinical diagnoses, prescribed medications with dosages, and printable prescription format (`Print Rx`).
- **Health Profile**: Manage personal contact details, emergency contacts, blood group, and residential address.

---

## 🔐 Default Login Credentials

For interviewers and portfolio reviewers, the login page (`/Account/Login`) includes **1-Click Quick Demo Login buttons** that automatically populate and authenticate any role:

| Role | Email | Password | Access Path | Description |
| :--- | :--- | :--- | :--- | :--- |
| **Admin** | `admin@medconnect.com` | `Admin@123` | `/Admin` | Complete hospital management & analytics |
| **Doctor** | `dr.smith@medconnect.com` | `Doctor@123` | `/Doctor` | Cardiology specialist schedule & EHR |
| **Doctor** | `dr.sarah@medconnect.com` | `Doctor@123` | `/Doctor` | Neurology specialist schedule & EHR |
| **Patient** | `john.doe@example.com` | `Patient@123` | `/Patient` | Established patient with existing records |
| **Patient** | `emily.watson@example.com` | `Patient@123` | `/Patient` | Patient with upcoming appointments |

---

## 🏛️ System Architecture

MedConnect is architected following industry-standard **Clean Architecture** principles:

```
my_demo_application.sln
│
├── 📂 my_demo_application.Domain          (Core Entities, Enums, Zero External Dependencies)
│   ├── Entities/   -> User, Doctor, Patient, Department, DoctorAvailability, Appointment, MedicalRecord
│   └── Enums/      -> UserRole, AppointmentStatus, Gender, BloodGroup
│
├── 📂 my_demo_application.Application     (Business Logic, Interfaces, DTOs & ViewModels)
│   ├── Interfaces/ -> IApplicationDbContext, IAuthService, IAdminService, IDoctorService, IPatientService
│   ├── Services/   -> AuthService, TokenService, AdminService, DoctorService, PatientService
│   └── ViewModels/ -> AuthViewModels, AdminViewModels, DoctorViewModels, PatientViewModels
│
├── 📂 my_demo_application.Infrastructure  (Persistence, EF Core, Repositories, Migrations)
│   ├── Data/         -> ApplicationDbContext, DbInitializer (Migrations & Auto-Seeding)
│   ├── Migrations/   -> EF Core Migration snapshots (InitialCreate)
│   └── Repositories/ -> Generic Repository<T>, UnitOfWork
│
└── 📂 my_demo_application                 (Presentation Layer - MVC & Web API)
    ├── Controllers/     -> AccountController, AdminController, DoctorController, PatientController
    ├── Controllers/Api/ -> AuthController, AdminApiController, DoctorsApiController, PatientsApiController
    ├── Views/           -> Responsive Bootstrap 5 + FontAwesome 6 Razor Views
    ├── Program.cs       -> DI, Dual Auth (Cookie + JWT Bearer), Swagger, CORS, Middleware
    └── appsettings.json -> SQL Server connection string & JWT configuration
```

---

## ⚙️ Business Rules Implemented

1. **Anti-Double Booking (Doctor Conflict)**: A doctor cannot have more than one confirmed or pending booking at the exact same date and time.
2. **Anti-Double Booking (Patient Conflict)**: A patient cannot book overlapping consultations at the same time slot across different doctors.
3. **Past Date Prevention**: The system rejects any appointment bookings or availability slot creations targeting dates prior to today.
4. **Availability Conflict Protection**: The slot generator prevents overlapping time ranges when defining doctor availability.
5. **State Transition Integrity**: Cancelled appointments cannot be marked as completed, and completed appointments cannot be cancelled.
6. **Clinical Authority**: Only the attending doctor assigned to an appointment can author the diagnosis, medical record, and prescription.
7. **Role-Based Route & API Protection**: Endpoints enforce strict role checks; doctors cannot access the admin portal, and patients cannot modify clinical availability.

---

## 🚀 Setup & Local Running Instructions

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or higher
- Microsoft SQL Server (LocalDB or SQLEXPRESS)
- SQL Server Management Studio (SSMS) or Azure Data Studio (optional)

### 1. Database Configuration
Open `my_demo_application/appsettings.json` and ensure the connection string points to your local SQL Server:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=Aditya\\SQLEXPRESS;Database=MedConnectDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
}
```
*(Note: If using LocalDB instead, change `Server=Aditya\SQLEXPRESS` to `Server=(localdb)\mssqllocaldb`)*.

### 2. Apply Migrations & Seed Database
Open terminal or PowerShell in the root project directory:

```bash
# Apply EF Core migrations to SQL Server
dotnet ef database update --project my_demo_application.Infrastructure --startup-project my_demo_application
```

### 3. Run the Application
```bash
# Navigate to the web application project
cd my_demo_application

# Run the ASP.NET Core server
dotnet run --launch-profile "http"
```

The application will start and output:
```
Now listening on: http://localhost:5208
Database migrated and seeded successfully.
```

Open your browser to:
- **Web Application Portal**: [http://localhost:5208](http://localhost:5208)
- **Interactive Swagger UI**: [http://localhost:5208/swagger](http://localhost:5208/swagger)

---

## 📡 REST API Catalog (Swagger Enabled)

Protected endpoints support JWT Bearer authorization (`Authorization: Bearer {token}`).

### Authentication
- `POST /api/auth/login` - Authenticate and receive JWT token + user claims
- `POST /api/auth/register` - Register a new patient account

### Admin Endpoints (`Bearer [Admin]`)
- `GET /api/admin/dashboard` - Retrieve aggregated executive metrics
- `GET /api/admin/doctors` - Filter and search doctor directory
- `GET /api/admin/patients` - List registered patients
- `GET /api/admin/departments` - List departments with doctor headcounts
- `GET /api/admin/appointments` - Master appointment schedule filter

### Doctor Endpoints (`Bearer [Doctor]`)
- `GET /api/doctors/profile` - Attending doctor profile
- `GET /api/doctors/appointments` - Filter appointments by today, upcoming, or completed
- `GET /api/doctors/availability` - Active availability slots

### Patient Endpoints (`Bearer [Patient]`)
- `GET /api/patients/doctors` - Public directory of verified specialists
- `GET /api/patients/doctors/{id}` - Specific doctor clinical credentials & bio
- `GET /api/patients/doctors/{id}/availability?date={date}` - Available time slots
- `POST /api/patients/appointments` - Reserve appointment slot with conflict validation
- `GET /api/patients/appointments` - Patient appointment history
- `GET /api/patients/medical-records` - Prescriptions and diagnoses

---

## 🎯 Technical Interview Talking Points

- **Clean Architecture & Inversion of Control**: The domain core remains completely pure with zero framework dependencies. Application services interact with data access strictly via the `IApplicationDbContext` interface and `IRepository<T>` contracts.
- **Dual Authentication**: Implements cookie authentication with sliding expirations for server-rendered MVC views while simultaneously hosting JWT Bearer authentication for stateless REST APIs.
- **Concurrency & Conflict Prevention**: Demonstrates race condition prevention through database indexes and LINQ validation guards against duplicate booking attempts.
- **Entity Framework Core Optimization**: Employs projection queries (`.Select()`), explicit eager loading (`.Include()`), and tracked vs untracked query patterns for performance.
