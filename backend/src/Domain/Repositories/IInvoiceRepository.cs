using Domain.Contracts;
using Domain.Enums;

namespace Domain.Repositories;

public interface IInvoiceRepository
{
    Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken ct);
    Task<InvoiceDto> CreateAsync(string description, decimal amount, Guid customerId, CancellationToken ct);
    Task<InvoiceDto> TransitionAsync(Guid id, InvoiceStatus newStatus, CancellationToken ct);
}