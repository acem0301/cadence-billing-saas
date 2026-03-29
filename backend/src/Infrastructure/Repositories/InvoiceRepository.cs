using Application.Abstractions;
using Domain.Contracts;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class InvoiceRepository(AppDbContext db, ITenantContext tenantContext) : IInvoiceRepository
{
    private readonly AppDbContext _db = db;
    private readonly ITenantContext _tenantContext = tenantContext;

    public async Task<IReadOnlyList<InvoiceDto>> GetAllAsync(CancellationToken ct)
    {
        return await _db.Invoices
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new InvoiceDto(
                x.Id, x.Description, x.Amount, x.Status,
                x.CreatedAt, x.PaidAt, x.CustomerId))
            .ToListAsync(ct);
    }

    public async Task<InvoiceDto> CreateAsync(
        string description, decimal amount, Guid customerId, CancellationToken ct)
    {
        var customerExists = await _db.Customers
            .AnyAsync(x => x.Id == customerId, ct);

        if (!customerExists)
            throw new InvoiceDomainException("Customer not found.");

        var entity = new Invoice
        {
            Id = Guid.NewGuid(),
            Description = description,
            Amount = amount,
            CustomerId = customerId,
            TenantId = _tenantContext.TenantId
        };

        _db.Invoices.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new InvoiceDto(
            entity.Id, entity.Description, entity.Amount, entity.Status,
            entity.CreatedAt, entity.PaidAt, entity.CustomerId);
    }

    public async Task<InvoiceDto> TransitionAsync(
        Guid id, InvoiceStatus newStatus, CancellationToken ct)
    {
        var invoice = await _db.Invoices
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (invoice is null)
            throw new InvoiceDomainException("Invoice not found.");

        // This is where the domain protects itself
        invoice.Transition(newStatus);

        await _db.SaveChangesAsync(ct);

        return new InvoiceDto(
            invoice.Id, invoice.Description, invoice.Amount, invoice.Status,
            invoice.CreatedAt, invoice.PaidAt, invoice.CustomerId);
    }
}