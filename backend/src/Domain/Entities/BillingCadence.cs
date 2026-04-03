using Domain.Enums;

namespace Domain.Entities;

public sealed class BillingCadence
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public BillingFrequency Frequency { get; set; }
    public DateOnly NextBillingDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    public void AdvanceNextBillingDate()
    {
        NextBillingDate = Frequency switch
        {
            BillingFrequency.Weekly => NextBillingDate.AddDays(7),
            BillingFrequency.BiWeekly => NextBillingDate.AddDays(15),
            BillingFrequency.Monthly => NextBillingDate.AddMonths(1),
            _ => throw new InvalidOperationException($"Unknown frequency: {Frequency}")
        };
    }
}