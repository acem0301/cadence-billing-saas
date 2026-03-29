using Domain.Enums;

namespace Domain.Contracts;

public sealed record InvoiceDto(
    Guid Id,
    string Description,
    decimal Amount,
    InvoiceStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? PaidAt,
    Guid CustomerId
);