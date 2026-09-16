using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_demo_application.Application.Interfaces;
using my_demo_application.Application.ViewModels;

namespace my_demo_application.Controllers;

[Authorize(Roles = "Doctor")]
public class DoctorController : Controller
{
    private readonly IDoctorService _doctorService;

    public DoctorController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    private int GetCurrentDoctorId()
    {
        var claim = User.FindFirst("DoctorId");
        if (claim != null && int.TryParse(claim.Value, out var docId))
        {
            return docId;
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var doc = _doctorService.GetDoctorByUserIdAsync(userId).GetAwaiter().GetResult();
        return doc?.DoctorId ?? throw new UnauthorizedAccessException("Doctor profile not found.");
    }

    public async Task<IActionResult> Index()
    {
        var doctorId = GetCurrentDoctorId();
        var model = await _doctorService.GetDashboardAsync(doctorId);
        return View(model);
    }

    public async Task<IActionResult> Appointments(string? filter)
    {
        var doctorId = GetCurrentDoctorId();
        ViewBag.CurrentFilter = filter ?? "all";
        var appointments = await _doctorService.GetAppointmentsAsync(doctorId, filter);
        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptAppointment(int id)
    {
        var doctorId = GetCurrentDoctorId();
        var result = await _doctorService.AcceptAppointmentAsync(id, doctorId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Appointments");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectAppointment(int id)
    {
        var doctorId = GetCurrentDoctorId();
        var result = await _doctorService.RejectAppointmentAsync(id, doctorId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Appointments");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteAppointment(int id)
    {
        var doctorId = GetCurrentDoctorId();
        var result = await _doctorService.CompleteAppointmentAsync(id, doctorId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Appointments");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var doctorId = GetCurrentDoctorId();
        var result = await _doctorService.CancelAppointmentAsync(id, doctorId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Appointments");
    }

    public async Task<IActionResult> Availability()
    {
        var doctorId = GetCurrentDoctorId();
        var slots = await _doctorService.GetAvailabilitiesAsync(doctorId);
        return View(slots);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateSlots(CreateAvailabilitySlotViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide valid date and start/end times.";
            return RedirectToAction("Availability");
        }

        var doctorId = GetCurrentDoctorId();
        var result = await _doctorService.GenerateSlotsAsync(doctorId, model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Availability");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSlot(int id)
    {
        var doctorId = GetCurrentDoctorId();
        var result = await _doctorService.DeleteSlotAsync(id, doctorId);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Availability");
    }

    [HttpGet]
    public async Task<IActionResult> AddMedicalRecord(int appointmentId, int patientId)
    {
        var doctorId = GetCurrentDoctorId();
        var appointments = await _doctorService.GetAppointmentsAsync(doctorId);
        var apt = appointments.FirstOrDefault(a => a.AppointmentId == appointmentId);
        if (apt == null) return NotFound();

        var model = new AddMedicalRecordViewModel
        {
            AppointmentId = appointmentId,
            PatientId = patientId,
            DoctorId = doctorId,
            PatientName = apt.PatientName,
            AppointmentDate = apt.AppointmentDate,
            Reason = apt.Reason
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMedicalRecord(AddMedicalRecordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var doctorId = GetCurrentDoctorId();
        var result = await _doctorService.AddMedicalRecordAsync(model, doctorId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = "Medical record saved and appointment completed successfully!";
        return RedirectToAction("Appointments");
    }

    public async Task<IActionResult> PatientHistory(int patientId)
    {
        var history = await _doctorService.GetPatientHistoryAsync(patientId);
        return View(history);
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var doctorId = GetCurrentDoctorId();
        var profile = await _doctorService.GetProfileAsync(doctorId);
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(DoctorProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var doctorId = GetCurrentDoctorId();
        model.DoctorId = doctorId;
        var result = await _doctorService.UpdateProfileAsync(model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Profile");
    }
}
