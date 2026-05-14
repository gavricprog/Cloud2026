using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.DTOs.Auth;
using SmartApiary.Application.DTOs.Users;
using SmartApiary.Application.Interfaces;
using SmartApiary.Domain.Entities;
using SmartApiary.Domain.Enums;
using SmartApiary.Infrastructure.Persistence;

namespace SmartApiary.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public UsersController(AppDbContext db, IEmailService emailService, IConfiguration config)
    {
        _db = db;
        _emailService = emailService;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role.ToString(),
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        return Ok(new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
            return BadRequest(new { message = "Nevažeća uloga. Dozvoljene vrednosti: Beekeeper, Farmer." });

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict(new { message = "Korisnik sa tim emailom već postoji." });

        var activationToken = Guid.NewGuid().ToString("N");
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Role = role,
            IsActive = false,
            PasswordHash = string.Empty,
            ActivationToken = activationToken,
            ActivationTokenExpiry = DateTime.UtcNow.AddHours(24)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        try
        {
            var activationLink = $"{_config["Frontend:BaseUrl"]}/activate?token={activationToken}";
            await _emailService.SendActivationEmailAsync(user.Email, $"{user.FirstName} {user.LastName}", activationLink);
        }
        catch { /* Email not configured in dev */ }

        return CreatedAtAction(nameof(GetById), new { id = user.Id }, new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString(),
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.IsActive = false;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:guid}/set-password")]
    public async Task<IActionResult> SetPassword(Guid id, [FromBody] SetPasswordRequest request)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.IsActive = true;
        user.ActivationToken = null;
        user.ActivationTokenExpiry = null;
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
