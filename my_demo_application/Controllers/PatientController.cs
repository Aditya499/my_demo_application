using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_demo_application.Application.Interfaces;
using my_demo_application.Application.ViewModels;

namespace my_demo_application.Controllers;

[Authorize(Roles = "Patient")]
public class PatientController : Controller
{
    private readonly IPatientService _patientService;
    private readonly IAdminService _adminService;

    public PatientController(IPatientService patientService, IAdminService adminService)
    {
        _patientService = patientService;
        _adminService = adminService;
    }

    private int GetCurrentPatientId()
    {
        var claim = User.FindFirst("PatientId");
        if (claim != null && int.TryParse(claim.Value, out var patId))
        {
            return patId;
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var pat = _patientService.GetPatientByUserIdAsync(userId).GetAwaiter().GetResult();
        return pat?.PatientId ?? throw new UnauthorizedAccessException("Patient profile not found.");
    }

    public async Task<IActionResult> Index()
    {
        var patientId = GetCurrentPatientId();
        var model = await _patientService.GetDashboardAsync(patientId);
        return View(model);
    }

    public async Task<IActionResult> FindDoctor(string? search, int? departmentId)
    {
        ViewBag.Departments = await _adminService.GetAllDepartmentsAsync();
        ViewBag.Search = search;
        ViewBag.SelectedDepartmentId = departmentId;
        var doctors = await _patientService.SearchDoctorsAsync(search, departmentId);
        return View(doctors);
    }

    public async Task<IActionResult> DoctorDetails(int id)
    {
        var doc = await _patientService.GetDoctorDetailsAsync(id);
        if (doc == null) return NotFound();
        return View(doc);
    }

    [HttpGet]
    public async Task<IActionResult> BookAppointment(int doctorId, DateTime? date = null)
    {
        var doc = await _patientService.GetDoctorDetailsAsync(doctorId);
        if (doc == null) return NotFound();

        var selectedDate = date ?? DateTime.Today.AddDays(1);
        var slots = await _patientService.GetDoctorAvailableSlotsAsync(doctorId, selectedDate);

        var model = new BookAppointmentViewModel
        {
            DoctorId = doctorId,
            DoctorName = doc.FullName,
            Specialization = doc.Specialization,
            DepartmentName = doc.DepartmentName,
            ConsultationFee = doc.ConsultationFee,
            AppointmentDate = selectedDate,
            AvailableSlots = slots
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetSlotsForDate(int doctorId, DateTime date)
    {
        var slots = await _patientService.GetDoctorAvailableSlotsAsync(doctorId, date);
        return Json(slots);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BookAppointment(BookAppointmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableSlots = await _patientService.GetDoctorAvailableSlotsAsync(model.DoctorId, model.AppointmentDate);
            return View(model);
        }

        var patientId = GetCurrentPatientId();
        var result = await _patientService.BookAppointmentAsync(patientId, model);

        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            model.AvailableSlots = await _patientService.GetDoctorAvailableSlotsAsync(model.DoctorId, model.AppointmentDate);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("MyAppointments");
    }

    public async Task<IActionResult> MyAppointments()
    {
        var patientId = GetCurrentPatientId();
        var appointments = await _patientService.GetMyAppointmentsAsync(patientId);
        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var patientId = GetCurrentPatientId();
        var result = await _patientService.CancelAppointmentAsync(id, patientId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("MyAppointments");
    }

    public async Task<IActionResult> MyMedicalRecords()
    {
        var patientId = GetCurrentPatientId();
        var records = await _patientService.GetMyMedicalRecordsAsync(patientId);
        return View(records);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var patientId = GetCurrentPatientId();
        var profile = await _patientService.GetProfileAsync(patientId);
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(PatientProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var patientId = GetCurrentPatientId();
        model.PatientId = patientId;
        var result = await _patientService.UpdateProfileAsync(model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Profile");
    }
}
