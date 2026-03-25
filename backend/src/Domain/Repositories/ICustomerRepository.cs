using Application.Contracts;

namespace Domain.Repositories;

public interface ICustomerRepository
{
    Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct);
    Task<CustomerDto> CreateAsync(string name, string? email, CancellationToken ct);
}