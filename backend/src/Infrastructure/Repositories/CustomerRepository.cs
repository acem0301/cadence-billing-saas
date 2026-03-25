using Application.Abstractions;
using Application.Contracts;
using Domain.Entities;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public CustomerRepository(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Customers
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new CustomerDto(x.Id, x.Name, x.Email, x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<CustomerDto> CreateAsync(string name, string? email, CancellationToken ct)
    {
        var entity = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            TenantId = _tenantContext.TenantId
        };

        _db.Customers.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new CustomerDto(entity.Id, entity.Name, entity.Email, entity.CreatedAt);
    }
}