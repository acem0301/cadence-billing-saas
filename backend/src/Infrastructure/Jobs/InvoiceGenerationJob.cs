using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

public sealed class InvoiceGenerationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<InvoiceGenerationJob> logger) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<InvoiceGenerationJob> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunAsync(stoppingToken);

            var delay = GetDelayUntilNextRun();
            _logger.LogInformation(
                "Invoice generation job completed. Next run in {Hours}h {Minutes}m.",
                delay.Hours, delay.Minutes);

            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var dueCadences = await db.BillingCadences
            .IgnoreQueryFilters()
            .Include(x => x.Customer)
            .Where(x => x.IsActive && x.NextBillingDate <= today)
            .ToListAsync(ct);

        if (dueCadences.Count == 0)
        {
            _logger.LogInformation("No billing cadences due today ({Date}).", today);
            return;
        }

        _logger.LogInformation(
            "Found {Count} billing cadence(s) due. Generating invoices...", dueCadences.Count);

        var invoices = dueCadences.Select(cadence =>
        {
            var invoice = new Invoice
            {
                Id = Guid.NewGuid(),
                Description = cadence.Description,
                Amount = cadence.Amount,
                CustomerId = cadence.CustomerId,
                TenantId = cadence.TenantId
            };

            cadence.AdvanceNextBillingDate();

            return invoice;
        }).ToList();

        db.Invoices.AddRange(invoices);
        await db.SaveChangesAsync(ct);

        _logger.LogInformation("Generated {Count} invoice(s) successfully.", invoices.Count);
    }

    private static TimeSpan GetDelayUntilNextRun()
    {
        var now = DateTime.UtcNow;
        var nextRun = now.Date.AddDays(1);
        return nextRun - now;
    }
}