using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_demo_application.Application.Interfaces;

namespace my_demo_application.Controllers.Api;

[ApiController]
[Route("api/doctors")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Doctor")]
public class DoctorsApiController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public DoctorsApiController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    private int GetDoctorId()
    {
        var claim = User.FindFirst("DoctorId");
        if (claim != null && int.TryParse(claim.Value, out var id)) return id;
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var doc = _doctorService.GetDoctorByUserIdAsync(userId).GetAwaiter().GetResult();
        return doc?.DoctorId ?? throw new UnauthorizedAccessException();
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile() => Ok(await _doctorService.GetProfileAsync(GetDoctorId()));

    [HttpGet("appointments")]
    public async Task<IActionResult> GetAppointments([FromQuery] string? filter) =>
        Ok(await _doctorService.GetAppointmentsAsync(GetDoctorId(), filter));

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability() =>
        Ok(await _doctorService.GetAvailabilitiesAsync(GetDoctorId()));
}
