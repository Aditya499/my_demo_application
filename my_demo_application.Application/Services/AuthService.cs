using Microsoft.EntityFrameworkCore;
using my_demo_application.Application.Common;
using my_demo_application.Application.Interfaces;
using my_demo_application.Application.ViewModels;
using my_demo_application.Domain.Entities;
using my_demo_application.Domain.Enums;

namespace my_demo_application.Application.Services;

public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly ITokenService _tokenService;

    public AuthService(IApplicationDbContext context, ITokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public async Task<User?> ValidateUserAsync(string email, string password)
    {
        var user = await _context.Users
            .Include(u => u.Doctor)
            .Include(u => u.Patient)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

        if (user == null) return null;

        bool verified = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return verified ? user : null;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterPatientAsync(RegisterPatientViewModel model)
    {
        // Check if email exists
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower()))
        {
            return ApiResponse<AuthResponseDto>.Fail("An account with this email address already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
        var user = new User
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim().ToLower(),
            PasswordHash = passwordHash,
            Role = UserRole.Patient,
            Phone = model.Phone,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var patient = new Patient
        {
            UserId = user.Id,
            DateOfBirth = model.DateOfBirth,
            Gender = model.Gender,
            BloodGroup = model.BloodGroup,
            Address = model.Address,
            EmergencyContact = model.EmergencyContact
        };

        _context.Patients.Add(patient);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateJwtToken(user, null, patient.PatientId);

        var response = new AuthResponseDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            Token = token,
            PatientId = patient.PatientId
        };

        return ApiResponse<AuthResponseDto>.Ok(response, "Registration successful!");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordViewModel model)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return ApiResponse<bool>.Fail("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
        {
            return ApiResponse<bool>.Fail("Incorrect current password.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Password changed successfully.");
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Doctor)
            .Include(u => u.Patient)
            .FirstOrDefaultAsync(u => u.Id == id);
    }
}
