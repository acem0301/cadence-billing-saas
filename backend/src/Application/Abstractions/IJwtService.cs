namespace Application.Abstractions;

public interface IJwtService
{
    string GenerateToken(Guid userId, Guid tenantId, string email);
}