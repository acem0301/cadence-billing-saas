using Domain.Contracts;
using Domain.Repositories;

namespace Application.Invoices.Commands;

public sealed class CreateInvoiceCommand(IInvoiceRepository repository)
{
    private readonly IInvoiceRepository _repository = repository;

    public sealed record Input(string Description, decimal Amount, Guid CustomerId);

    public Task<InvoiceDto> ExecuteAsync(Input input, CancellationToken ct)
        => _repository.CreateAsync(input.Description, input.Amount, input.CustomerId, ct);
}