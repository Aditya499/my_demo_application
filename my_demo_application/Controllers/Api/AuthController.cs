using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using my_demo_application.Application.Common;
using my_demo_application.Application.Interfaces;
using my_demo_application.Application.ViewModels;

namespace my_demo_application.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenService _tokenService;

    public AuthController(IAuthService authService, ITokenService tokenService)
    {
        _authService = authService;
        _tokenService = tokenService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginViewModel model)
    {
        var user = await _authService.ValidateUserAsync(model.Email, model.Password);
        if (user == null)
        {
            return Unauthorized(ApiResponse<AuthResponseDto>.Fail("Invalid email or password."));
        }

        var token = _tokenService.GenerateJwtToken(user, user.Doctor?.DoctorId, user.Patient?.PatientId);
        var response = new AuthResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = token,
            DoctorId = user.Doctor?.DoctorId,
            PatientId = user.Patient?.PatientId
        };

        return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Login successful."));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterPatientViewModel model)
    {
        var result = await _authService.RegisterPatientAsync(model);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
}
