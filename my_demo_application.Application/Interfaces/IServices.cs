using my_demo_application.Application.Common;
using my_demo_application.Application.ViewModels;
using my_demo_application.Domain.Entities;

namespace my_demo_application.Application.Interfaces;

public interface IAuthService
{
    Task<User?> ValidateUserAsync(string email, string password);
    Task<ApiResponse<AuthResponseDto>> RegisterPatientAsync(RegisterPatientViewModel model);
    Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
    Task<User?> GetUserByIdAsync(int id);
}

public interface ITokenService
{
    string GenerateJwtToken(User user, int? doctorId = null, int? patientId = null);
}

public interface IAdminService
{
    Task<AdminDashboardViewModel> GetDashboardStatsAsync();
    Task<IEnumerable<DoctorCreateEditViewModel>> GetAllDoctorsAsync(string? search = null, int? departmentId = null);
    Task<ApiResponse<int>> CreateDoctorAsync(DoctorCreateEditViewModel model);
    Task<ApiResponse<bool>> UpdateDoctorAsync(DoctorCreateEditViewModel model);
    Task<ApiResponse<bool>> ToggleDoctorStatusAsync(int doctorId);
    Task<IEnumerable<PatientListItemViewModel>> GetAllPatientsAsync(string? search = null);
    Task<IEnumerable<DepartmentViewModel>> GetAllDepartmentsAsync();
    Task<ApiResponse<int>> CreateDepartmentAsync(DepartmentViewModel model);
    Task<ApiResponse<bool>> UpdateDepartmentAsync(DepartmentViewModel model);
    Task<ApiResponse<bool>> DeleteDepartmentAsync(int departmentId);
    Task<IEnumerable<RecentAppointmentViewModel>> GetAllAppointmentsAsync(string? status = null, DateTime? date = null);
    Task<ApiResponse<bool>> UpdateAppointmentStatusAsync(int appointmentId, string newStatus);
}

public interface IDoctorService
{
    Task<Doctor?> GetDoctorByUserIdAsync(int userId);
    Task<DoctorDashboardViewModel> GetDashboardAsync(int doctorId);
    Task<DoctorProfileViewModel> GetProfileAsync(int doctorId);
    Task<ApiResponse<bool>> UpdateProfileAsync(DoctorProfileViewModel model);
    Task<IEnumerable<DoctorAppointmentItemViewModel>> GetAppointmentsAsync(int doctorId, string? filter = null);
    Task<ApiResponse<bool>> AcceptAppointmentAsync(int appointmentId, int doctorId);
    Task<ApiResponse<bool>> RejectAppointmentAsync(int appointmentId, int doctorId);
    Task<ApiResponse<bool>> CompleteAppointmentAsync(int appointmentId, int doctorId);
    Task<ApiResponse<bool>> CancelAppointmentAsync(int appointmentId, int doctorId);
    Task<IEnumerable<DoctorAvailability>> GetAvailabilitiesAsync(int doctorId, DateTime? fromDate = null);
    Task<ApiResponse<int>> GenerateSlotsAsync(int doctorId, CreateAvailabilitySlotViewModel model);
    Task<ApiResponse<bool>> DeleteSlotAsync(int availabilityId, int doctorId);
    Task<ApiResponse<int>> AddMedicalRecordAsync(AddMedicalRecordViewModel model, int doctorId);
    Task<IEnumerable<MedicalRecordItemViewModel>> GetPatientHistoryAsync(int patientId);
}

public interface IPatientService
{
    Task<Patient?> GetPatientByUserIdAsync(int userId);
    Task<PatientDashboardViewModel> GetDashboardAsync(int patientId);
    Task<PatientProfileViewModel> GetProfileAsync(int patientId);
    Task<ApiResponse<bool>> UpdateProfileAsync(PatientProfileViewModel model);
    Task<IEnumerable<DoctorDirectoryItemViewModel>> SearchDoctorsAsync(string? search = null, int? departmentId = null);
    Task<DoctorDirectoryItemViewModel?> GetDoctorDetailsAsync(int doctorId);
    Task<List<SlotItemViewModel>> GetDoctorAvailableSlotsAsync(int doctorId, DateTime date);
    Task<ApiResponse<int>> BookAppointmentAsync(int patientId, BookAppointmentViewModel model);
    Task<IEnumerable<PatientAppointmentItemViewModel>> GetMyAppointmentsAsync(int patientId);
    Task<ApiResponse<bool>> CancelAppointmentAsync(int appointmentId, int patientId);
    Task<IEnumerable<MedicalRecordItemViewModel>> GetMyMedicalRecordsAsync(int patientId);
}
