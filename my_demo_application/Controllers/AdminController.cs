using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_demo_application.Application.Interfaces;
using my_demo_application.Application.ViewModels;

namespace my_demo_application.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _adminService.GetDashboardStatsAsync();
        return View(model);
    }

    public async Task<IActionResult> Doctors(string? search, int? departmentId)
    {
        ViewBag.Departments = await _adminService.GetAllDepartmentsAsync();
        ViewBag.Search = search;
        ViewBag.SelectedDepartmentId = departmentId;
        var doctors = await _adminService.GetAllDoctorsAsync(search, departmentId);
        return View(doctors);
    }

    [HttpGet]
    public async Task<IActionResult> CreateDoctor()
    {
        ViewBag.Departments = await _adminService.GetAllDepartmentsAsync();
        return View(new DoctorCreateEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDoctor(DoctorCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _adminService.GetAllDepartmentsAsync();
            return View(model);
        }

        var result = await _adminService.CreateDoctorAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            ViewBag.Departments = await _adminService.GetAllDepartmentsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Doctor created successfully!";
        return RedirectToAction("Doctors");
    }

    [HttpGet]
    public async Task<IActionResult> EditDoctor(int id)
    {
        var doctors = await _adminService.GetAllDoctorsAsync();
        var doctor = doctors.FirstOrDefault(d => d.DoctorId == id);
        if (doctor == null) return NotFound();

        ViewBag.Departments = await _adminService.GetAllDepartmentsAsync();
        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDoctor(DoctorCreateEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Departments = await _adminService.GetAllDepartmentsAsync();
            return View(model);
        }

        var result = await _adminService.UpdateDoctorAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            ViewBag.Departments = await _adminService.GetAllDepartmentsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Doctor updated successfully!";
        return RedirectToAction("Doctors");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleDoctorStatus(int id)
    {
        var result = await _adminService.ToggleDoctorStatusAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Doctors");
    }

    public async Task<IActionResult> Patients(string? search)
    {
        ViewBag.Search = search;
        var patients = await _adminService.GetAllPatientsAsync(search);
        return View(patients);
    }

    public async Task<IActionResult> Departments()
    {
        var depts = await _adminService.GetAllDepartmentsAsync();
        return View(depts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDepartment(DepartmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Please provide a valid department name.";
            return RedirectToAction("Departments");
        }

        var result = await _adminService.CreateDepartmentAsync(model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Departments");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDepartment(DepartmentViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid department details.";
            return RedirectToAction("Departments");
        }

        var result = await _adminService.UpdateDepartmentAsync(model);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Departments");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var result = await _adminService.DeleteDepartmentAsync(id);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Departments");
    }

    public async Task<IActionResult> Appointments(string? status, DateTime? date)
    {
        ViewBag.SelectedStatus = status;
        ViewBag.SelectedDate = date?.ToString("yyyy-MM-dd");
        var appointments = await _adminService.GetAllAppointmentsAsync(status, date);
        return View(appointments);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateAppointmentStatus(int id, string newStatus)
    {
        var result = await _adminService.UpdateAppointmentStatusAsync(id, newStatus);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Appointments");
    }

    public async Task<IActionResult> Reports()
    {
        var stats = await _adminService.GetDashboardStatsAsync();
        return View(stats);
    }
}
