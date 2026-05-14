using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.DTOs.Auth;
using SmartApiary.Application.Interfaces;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext db, ITokenService tokenService, IEmailService emailService, IConfiguration config)
    {
        _db = db;
        _tokenService = tokenService;
        _emailService = emailService;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Pogrešan email ili lozinka." });

        var token = _tokenService.GenerateJwtToken(user);
        return Ok(new LoginResponse
        {
            Token = token,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Role = user.Role.ToString()
        });
    }

    [HttpPost("activate")]
    public async Task<IActionResult> ActivateAccount([FromBody] SetPasswordRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { message = "Lozinke se ne podudaraju." });

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.ActivationToken == request.Token &&
            u.ActivationTokenExpiry > DateTime.UtcNow);

        if (user == null)
            return BadRequest(new { message = "Nevažeći ili istekli token." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.IsActive = true;
        user.ActivationToken = null;
        user.ActivationTokenExpiry = null;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Nalog je uspešno aktiviran." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user == null)
            return Ok(new { message = "Ukoliko nalog postoji, poslaćemo email." });

        user.PasswordResetToken = Guid.NewGuid().ToString("N");
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _db.SaveChangesAsync();

        var resetLink = $"{_config["Frontend:BaseUrl"]}/reset-password?token={user.PasswordResetToken}";
        await _emailService.SendPasswordResetEmailAsync(user.Email, $"{user.FirstName} {user.LastName}", resetLink);

        return Ok(new { message = "Ukoliko nalog postoji, poslaćemo email." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] SetPasswordRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            return BadRequest(new { message = "Lozinke se ne podudaraju." });

        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == request.Token &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (user == null)
            return BadRequest(new { message = "Nevažeći ili istekli token." });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Lozinka je uspešno promenjena." });
    }
}
