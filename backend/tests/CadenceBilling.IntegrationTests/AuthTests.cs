using FluentAssertions;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace CadenceBilling.IntegrationTests;

public sealed class AuthTests(IntegrationTestFactory factory)
    : IClassFixture<IntegrationTestFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_WithValidData_ReturnsToken()
    {
        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            companyName = "Acme Corp",
            email = $"{Guid.NewGuid()}@acme.com",
            password = "Secret123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var email = $"{Guid.NewGuid()}@acme.com";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            companyName = "Acme Corp",
            email,
            password = "Secret123!"
        });

        var response = await _client.PostAsJsonAsync("/auth/register", new
        {
            companyName = "Acme Corp",
            email,
            password = "Secret123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var email = $"{Guid.NewGuid()}@acme.com";
        var password = "Secret123!";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            companyName = "Acme Corp",
            email,
            password
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new { email, password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TokenResponse>();
        body!.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var email = $"{Guid.NewGuid()}@acme.com";

        await _client.PostAsJsonAsync("/auth/register", new
        {
            companyName = "Acme Corp",
            email,
            password = "Secret123!"
        });

        var response = await _client.PostAsJsonAsync("/auth/login", new
        {
            email,
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record TokenResponse(string Token);
}