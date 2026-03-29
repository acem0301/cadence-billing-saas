using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public sealed class Invoice
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public decimal Amount { get; set; }
    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PaidAt { get; private set; }

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public Guid TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    private static readonly Dictionary<InvoiceStatus, InvoiceStatus[]> _allowedTransitions = new()
    {
        [InvoiceStatus.Draft] = [InvoiceStatus.Approved, InvoiceStatus.Cancelled],
        [InvoiceStatus.Approved] = [InvoiceStatus.Sent, InvoiceStatus.Cancelled],
        [InvoiceStatus.Sent] = [InvoiceStatus.Paid, InvoiceStatus.Cancelled],
        [InvoiceStatus.Paid] = [],
        [InvoiceStatus.Cancelled] = []
    };

    public void Transition(InvoiceStatus newStatus)
    {
        if (!_allowedTransitions[Status].Contains(newStatus))
            throw new InvoiceDomainException(
                $"Cannot transition invoice from '{Status}' to '{newStatus}'.");

        Status = newStatus;

        if (newStatus == InvoiceStatus.Paid)
            PaidAt = DateTimeOffset.UtcNow;
    }
}