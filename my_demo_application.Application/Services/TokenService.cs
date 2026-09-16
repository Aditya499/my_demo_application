using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using my_demo_application.Application.Interfaces;
using my_demo_application.Domain.Entities;

namespace my_demo_application.Application.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateJwtToken(User user, int? doctorId = null, int? patientId = null)
    {
        var keyStr = _config["JwtSettings:SecretKey"] ?? "MedConnectSuperSecretKeyForProduction2026!@#$%^&*";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (doctorId.HasValue) claims.Add(new Claim("DoctorId", doctorId.Value.ToString()));
        if (patientId.HasValue) claims.Add(new Claim("PatientId", patientId.Value.ToString()));

        var token = new JwtSecurityToken(
            issuer: _config["JwtSettings:Issuer"] ?? "MedConnect",
            audience: _config["JwtSettings:Audience"] ?? "MedConnectUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
