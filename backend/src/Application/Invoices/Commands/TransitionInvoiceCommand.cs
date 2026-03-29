using Domain.Contracts;
using Domain.Enums;
using Domain.Repositories;

namespace Application.Invoices.Commands;

public sealed class TransitionInvoiceCommand(IInvoiceRepository repository)
{
    private readonly IInvoiceRepository _repository = repository;

    public sealed record Input(Guid InvoiceId, InvoiceStatus NewStatus);

    public Task<InvoiceDto> ExecuteAsync(Input input, CancellationToken ct)
        => _repository.TransitionAsync(input.InvoiceId, input.NewStatus, ct);
}