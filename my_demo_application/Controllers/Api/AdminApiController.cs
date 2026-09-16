using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_demo_application.Application.Interfaces;

namespace my_demo_application.Controllers.Api;

[ApiController]
[Route("api/admin")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class AdminApiController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminApiController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard() => Ok(await _adminService.GetDashboardStatsAsync());

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors([FromQuery] string? search, [FromQuery] int? departmentId) =>
        Ok(await _adminService.GetAllDoctorsAsync(search, departmentId));

    [HttpGet("patients")]
    public async Task<IActionResult> GetPatients([FromQuery] string? search) =>
        Ok(await _adminService.GetAllPatientsAsync(search));

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments() =>
        Ok(await _adminService.GetAllDepartmentsAsync());

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments([FromQuery] string? status, [FromQuery] DateTime? date) =>
        Ok(await _adminService.GetAllAppointmentsAsync(status, date));
}
