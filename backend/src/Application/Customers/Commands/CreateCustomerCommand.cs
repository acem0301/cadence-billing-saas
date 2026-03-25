using Application.Contracts;
using Domain.Repositories;

namespace Application.Customers.Commands;

public sealed class CreateCustomerCommand
{
    private readonly ICustomerRepository _repository;

    public CreateCustomerCommand(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public sealed record Input(string Name, string? Email);

    public Task<CustomerDto> ExecuteAsync(Input input, CancellationToken ct)
        => _repository.CreateAsync(input.Name, input.Email, ct);
}