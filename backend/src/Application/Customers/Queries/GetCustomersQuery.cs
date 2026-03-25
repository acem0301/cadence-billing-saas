using Application.Contracts;
using Domain.Repositories;

namespace Application.Customers.Queries;

public sealed class GetCustomersQuery
{
    private readonly ICustomerRepository _repository;

    public GetCustomersQuery(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<CustomerDto>> ExecuteAsync(CancellationToken ct)
        => _repository.GetAllAsync(ct);
}