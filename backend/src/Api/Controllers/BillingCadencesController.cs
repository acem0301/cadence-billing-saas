using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Authorize]
[ApiController]
[Route("billing-cadences")]
public sealed class BillingCadencesController(
    AppDbContext db,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await db.BillingCadences
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.Description,
                x.Amount,
                x.Frequency,
                x.NextBillingDate,
                x.IsActive,
                x.CustomerId
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    public sealed record CreateBillingCadenceRequest(
        string Description,
        decimal Amount,
        BillingFrequency Frequency,
        DateOnly NextBillingDate,
        Guid CustomerId);

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateBillingCadenceRequest req, CancellationToken ct)
    {
        var customerExists = await db.Customers
            .AnyAsync(x => x.Id == req.CustomerId, ct);

        if (!customerExists)
            return NotFound(new { message = "Customer not found." });

        var entity = new BillingCadence
        {
            Id = Guid.NewGuid(),
            Description = req.Description,
            Amount = req.Amount,
            Frequency = req.Frequency,
            NextBillingDate = req.NextBillingDate,
            CustomerId = req.CustomerId,
            TenantId = tenantContext.TenantId
        };

        db.BillingCadences.Add(entity);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAll), new { id = entity.Id }, new { entity.Id });
    }
}