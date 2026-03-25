using Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services;

public sealed class TenantContext : ITenantContext
{
    public static readonly Guid DevTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?.User
                .FindFirst("tenantId")?.Value;

            return claim is not null
                ? Guid.Parse(claim)
                : DevTenantId;
        }
    }
}