using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OnlineParkingLotSystem.Application.DTOs.Requests;
using OnlineParkingLotSystem.Application.DTOs.Responses;
using OnlineParkingLotSystem.Domain.Exceptions;

namespace OnlineParkingLotSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var configuredUsername = _configuration["Auth:Username"];
        var configuredPassword = _configuration["Auth:Password"];

        if (!string.Equals(request.Username, configuredUsername, StringComparison.Ordinal) ||
            !string.Equals(request.Password, configuredPassword, StringComparison.Ordinal))
        {
            throw new InvalidCredentialsException();
        }

        var expiryMinutes = _configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);
        var token = GenerateToken(request.Username, expiresAt);

        return Task.FromResult(new AuthResponse
        {
            Token = token,
            ExpiresAt = expiresAt
        });
    }

    private string GenerateToken(string username, DateTime expiresAt)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
