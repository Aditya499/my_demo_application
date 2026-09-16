using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_demo_application.Application.Interfaces;
using my_demo_application.Application.ViewModels;

namespace my_demo_application.Controllers.Api;

[ApiController]
[Route("api/patients")]
public class PatientsApiController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsApiController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    private int GetPatientId()
    {
        var claim = User.FindFirst("PatientId");
        if (claim != null && int.TryParse(claim.Value, out var id)) return id;
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var pat = _patientService.GetPatientByUserIdAsync(userId).GetAwaiter().GetResult();
        return pat?.PatientId ?? throw new UnauthorizedAccessException();
    }

    [HttpGet("doctors")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDoctors([FromQuery] string? search, [FromQuery] int? departmentId) =>
        Ok(await _patientService.SearchDoctorsAsync(search, departmentId));

    [HttpGet("doctors/{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDoctor(int id)
    {
        var doc = await _patientService.GetDoctorDetailsAsync(id);
        return doc == null ? NotFound() : Ok(doc);
    }

    [HttpGet("doctors/{id}/availability")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDoctorAvailability(int id, [FromQuery] DateTime date) =>
        Ok(await _patientService.GetDoctorAvailableSlotsAsync(id, date));

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Patient")]
    [HttpPost("appointments")]
    public async Task<IActionResult> BookAppointment([FromBody] BookAppointmentViewModel model)
    {
        var result = await _patientService.BookAppointmentAsync(GetPatientId(), model);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Patient")]
    [HttpGet("appointments")]
    public async Task<IActionResult> GetMyAppointments() =>
        Ok(await _patientService.GetMyAppointmentsAsync(GetPatientId()));

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Patient")]
    [HttpGet("medical-records")]
    public async Task<IActionResult> GetMedicalRecords() =>
        Ok(await _patientService.GetMyMedicalRecordsAsync(GetPatientId()));
}
