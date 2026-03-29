using Domain.Contracts;
using Domain.Repositories;

namespace Application.Invoices.Queries;

public sealed class GetInvoicesQuery(IInvoiceRepository repository)
{
    private readonly IInvoiceRepository _repository = repository;

    public Task<IReadOnlyList<InvoiceDto>> ExecuteAsync(CancellationToken ct)
        => _repository.GetAllAsync(ct);
}