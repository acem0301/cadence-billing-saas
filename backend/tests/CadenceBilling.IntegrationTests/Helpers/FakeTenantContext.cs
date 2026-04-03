using Application.Abstractions;
using System;

namespace CadenceBilling.IntegrationTests.Helpers;

public sealed class FakeTenantContext(Guid tenantId) : ITenantContext
{
    public Guid TenantId { get; } = tenantId;
}