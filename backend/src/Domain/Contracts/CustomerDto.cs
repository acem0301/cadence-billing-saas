namespace Application.Contracts;

public sealed record CustomerDto(
    Guid Id,
    string Name,
    string? Email,
    DateTimeOffset CreatedAt
);