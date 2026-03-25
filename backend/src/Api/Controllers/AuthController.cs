using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(AppDbContext db, IJwtService jwtService) : ControllerBase
{
    public sealed record RegisterRequest(string CompanyName, string Email, string Password);
    public sealed record LoginRequest(string Email, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
    {
        var emailExists = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(x => x.Email == req.Email, ct);

        if (emailExists)
            return Conflict(new { message = "Email already in use." });

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = req.CompanyName
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = req.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
            TenantId = tenant.Id
        };

        db.Tenants.Add(tenant);
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        var token = jwtService.GenerateToken(user.Id, tenant.Id, user.Email);
        return Ok(new { token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Email == req.Email, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials." });

        var token = jwtService.GenerateToken(user.Id, user.TenantId, user.Email);
        return Ok(new { token });
    }
}